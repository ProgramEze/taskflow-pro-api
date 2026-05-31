using TaskFlowPro.Domain.Entities;

namespace TaskFlowPro.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);

    Task<User?> GetByEmailAsync(string email);

    Task AddAsync(User user);
}