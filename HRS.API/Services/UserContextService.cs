using HRS.Shared.Core.Dtos;
using HRS.Shared.Core.Interfaces;
using System.Security.Claims;

namespace HRS.API.Services;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    public int GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                         ?? _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    public Task<UserResponseDto> GetUserAsync()
    {
        var userId = GetUserId();
        var email = GetEmail();
        var firstName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.GivenName)?.Value
                       ?? _httpContextAccessor.HttpContext?.User?.FindFirst("firstName")?.Value
                       ?? "Unknown";
        var lastName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Surname)?.Value
                      ?? _httpContextAccessor.HttpContext?.User?.FindFirst("lastName")?.Value
                      ?? "User";
        var role = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value
                  ?? _httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value
                  ?? "User";

        // Create a user DTO from claims
        var user = new UserResponseDto
        {
            Id = userId,
            Email = email ?? "unknown@example.com",
            FirstName = firstName,
            LastName = lastName,
            Role = role
        };

        return Task.FromResult(user);
    }
}
