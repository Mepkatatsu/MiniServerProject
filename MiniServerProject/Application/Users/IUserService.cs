using MiniServerProject.Shared.Responses;

namespace MiniServerProject.Application.Users
{
    public interface IUserService
    {
        Task<UserResponse> CreateAsync(string accountId, string nickname, CancellationToken ct);
        Task<UserResponse> GetAsync(string accountId, CancellationToken ct);
    }
}
