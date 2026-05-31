using Microsoft.EntityFrameworkCore;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Entities;
using TaskFlowPro.Infrastructure.Data;

namespace TaskFlowPro.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(TaskItem task)
    {
        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _context.TaskItems.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TaskItem>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.TaskItems
            .Where(task => task.ProjectId == projectId && task.IsActive)
            .OrderByDescending(task => task.CreatedAt)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(Guid taskId)
    {
        return await _context.TaskItems
            .Include(task => task.Project)
                .ThenInclude(project => project.Workspace)
                    .ThenInclude(workspace => workspace.Members)
            .FirstOrDefaultAsync(task => task.Id == taskId);
    }
}