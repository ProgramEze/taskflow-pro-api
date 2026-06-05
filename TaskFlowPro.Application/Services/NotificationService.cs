using TaskFlowPro.Application.DTOs.Notifications;
using TaskFlowPro.Application.Exceptions;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Entities;

namespace TaskFlowPro.Application.Services;

public class NotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<List<NotificationDto>> GetByUserIdAsync(Guid userId)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);

        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id ?? string.Empty,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();
    }

    public async Task CreateAsync(CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message
        };

        await _notificationRepository.CreateAsync(notification);
    }

    public async Task MarkAsReadAsync(string id)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);

        if (notification is null)
            throw new NotFoundException("Notificación no encontrada.");

        await _notificationRepository.MarkAsReadAsync(id);
    }

    public async Task DeleteAsync(string id)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);

        if (notification is null)
            throw new NotFoundException("Notificación no encontrada.");

        await _notificationRepository.DeleteAsync(id);
    }
}