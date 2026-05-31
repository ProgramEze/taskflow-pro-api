using Microsoft.EntityFrameworkCore;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Entities;
using TaskFlowPro.Infrastructure.Data;

namespace TaskFlowPro.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Comment comment)
    {
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Comment>> GetByTaskIdAsync(Guid taskId)
    {
        return await _context.Comments
            .Where(comment =>
                comment.TaskItemId == taskId &&
                comment.IsActive)
            .OrderBy(comment => comment.CreatedAt)
            .ToListAsync();
    }

    public async Task<Comment?> GetByIdAsync(Guid commentId)
    {
        return await _context.Comments
            .Include(comment => comment.TaskItem)
                .ThenInclude(task => task.Project)
                    .ThenInclude(project => project.Workspace)
                        .ThenInclude(workspace => workspace.Members)
            .FirstOrDefaultAsync(comment => comment.Id == commentId);
    }
}