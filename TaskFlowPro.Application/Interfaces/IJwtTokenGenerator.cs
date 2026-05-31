using TaskFlowPro.Domain.Entities;

namespace TaskFlowPro.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}