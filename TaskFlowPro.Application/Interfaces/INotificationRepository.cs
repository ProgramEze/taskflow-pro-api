using TaskFlowPro.Domain.Entities;

namespace TaskFlowPro.Application.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>> GetByUserIdAsync(Guid userId);
    Task<Notification?> GetByIdAsync(string id);
    Task CreateAsync(Notification notification);
    Task MarkAsReadAsync(string id);
    Task DeleteAsync(string id);
}