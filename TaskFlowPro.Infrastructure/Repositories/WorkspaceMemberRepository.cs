using Microsoft.EntityFrameworkCore;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Entities;
using TaskFlowPro.Infrastructure.Data;

namespace TaskFlowPro.Infrastructure.Repositories;

public class WorkspaceMemberRepository : IWorkspaceMemberRepository
{
    private readonly AppDbContext _context;

    public WorkspaceMemberRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(WorkspaceMember member)
    {
        _context.WorkspaceMembers.Add(member);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(WorkspaceMember member)
    {
        _context.WorkspaceMembers.Update(member);
        await _context.SaveChangesAsync();
    }

    public async Task<List<WorkspaceMember>> GetByWorkspaceIdAsync(Guid workspaceId)
    {
        return await _context.WorkspaceMembers
            .Include(member => member.User)
            .Where(member => member.WorkspaceId == workspaceId)
            .OrderBy(member => member.JoinedAt)
            .ToListAsync();
    }

    public async Task<WorkspaceMember?> GetByIdAsync(Guid memberId)
    {
        return await _context.WorkspaceMembers
            .Include(member => member.User)
            .FirstOrDefaultAsync(member => member.Id == memberId);
    }

    public async Task<WorkspaceMember?> GetByWorkspaceAndUserIdAsync(Guid workspaceId, Guid userId)
    {
        return await _context.WorkspaceMembers
            .Include(member => member.User)
            .FirstOrDefaultAsync(member =>
                member.WorkspaceId == workspaceId &&
                member.UserId == userId);
    }
}