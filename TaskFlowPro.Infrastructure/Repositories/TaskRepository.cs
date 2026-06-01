using Microsoft.EntityFrameworkCore;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Entities;
using TaskFlowPro.Infrastructure.Data;
using TaskFlowPro.Application.DTOs.Common;
using TaskFlowPro.Application.DTOs.Tasks;

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

    public async Task<PagedResult<TaskItem>> GetByProjectIdPagedAsync(
    Guid projectId,
    TaskQueryParameters query)
    {
        var tasksQuery = _context.TaskItems
            .Where(task => task.ProjectId == projectId && task.IsActive)
            .AsQueryable();

        if (query.Status.HasValue)
        {
            tasksQuery = tasksQuery.Where(task => task.Status == query.Status.Value);
        }

        if (query.Priority.HasValue)
        {
            tasksQuery = tasksQuery.Where(task => task.Priority == query.Priority.Value);
        }

        if (query.AssignedUserId.HasValue)
        {
            tasksQuery = tasksQuery.Where(task => task.AssignedUserId == query.AssignedUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();

            tasksQuery = tasksQuery.Where(task =>
                task.Title.ToLower().Contains(search) ||
                (task.Description != null && task.Description.ToLower().Contains(search))
            );
        }

        var totalItems = await tasksQuery.CountAsync();

        var items = await tasksQuery
            .OrderByDescending(task => task.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<TaskItem>
        {
            Items = items,
            TotalItems = totalItems
        };
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