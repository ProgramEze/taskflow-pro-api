using Microsoft.EntityFrameworkCore;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Entities;
using TaskFlowPro.Infrastructure.Data;

namespace TaskFlowPro.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Project>> GetByWorkspaceIdAsync(Guid workspaceId)
    {
        return await _context.Projects
            .Where(project => project.WorkspaceId == workspaceId)
            .OrderByDescending(project => project.CreatedAt)
            .ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(Guid projectId)
    {
        return await _context.Projects
            .Include(project => project.Workspace)
                .ThenInclude(workspace => workspace.Members)
            .FirstOrDefaultAsync(project => project.Id == projectId);
    }
}