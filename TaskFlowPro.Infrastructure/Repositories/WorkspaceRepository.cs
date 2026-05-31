using Microsoft.EntityFrameworkCore;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Entities;
using TaskFlowPro.Infrastructure.Data;

namespace TaskFlowPro.Infrastructure.Repositories;

public class WorkspaceRepository : IWorkspaceRepository
{
    private readonly AppDbContext _context;

    public WorkspaceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Workspace workspace)
    {
        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Workspace>> GetByUserIdAsync(Guid userId)
    {
        return await _context.WorkspaceMembers
            .Where(member => member.UserId == userId && member.IsActive)
            .Include(member => member.Workspace)
            .Where(member => member.Workspace.IsActive)
            .Select(member => member.Workspace)
            .ToListAsync();
    }

    public async Task<Workspace?> GetByIdAsync(Guid workspaceId)
    {
        return await _context.Workspaces
            .Include(workspace => workspace.Members)
            .FirstOrDefaultAsync(workspace => workspace.Id == workspaceId);
    }
}