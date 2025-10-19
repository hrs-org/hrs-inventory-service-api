using HRS.Shared.Core.Dtos;
using HRS.Shared.Core.Interfaces;

namespace HRS.API.Services;

public class UserContextService : IUserContextService
{
    public string? GetEmail() => throw new NotImplementedException();
    public Task<UserResponseDto> GetUserAsync() => throw new NotImplementedException();
    public int GetUserId() => throw new NotImplementedException();
}
