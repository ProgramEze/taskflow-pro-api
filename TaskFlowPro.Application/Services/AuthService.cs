using TaskFlowPro.Application.DTOs.Auth;
using TaskFlowPro.Application.Exceptions;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Entities;

namespace TaskFlowPro.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new BadRequestException("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new BadRequestException("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BadRequestException("El email es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BadRequestException("La contraseña es obligatoria.");

        if (request.Password.Length < 6)
            throw new BadRequestException("La contraseña debe tener al menos 6 caracteres.");

        var email = request.Email.Trim().ToLower();

        var existingUser = await _userRepository.GetByEmailAsync(email);

        if (existingUser is not null)
            throw new ConflictException("Ya existe un usuario registrado con ese email.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepository.AddAsync(user);

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Token = token
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BadRequestException("El email es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BadRequestException("La contraseña es obligatoria.");

        var email = request.Email.Trim().ToLower();

        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null)
            throw new UnauthorizedException("Email o contraseña incorrectos.");

        if (!user.IsActive)
            throw new UnauthorizedException("El usuario se encuentra desactivado.");

        var passwordIsValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash
        );

        if (!passwordIsValid)
            throw new UnauthorizedException("Email o contraseña incorrectos.");

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Token = token
        };
    }

    public async Task<AuthResponse> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            throw new NotFoundException("Usuario no encontrado.");

        if (!user.IsActive)
            throw new UnauthorizedException("El usuario se encuentra desactivado.");

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Token = token
        };
    }
}