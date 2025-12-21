using MiniServerProject.Controllers.Request;
using MiniServerProject.Controllers.Response;

namespace MiniServerProject.Application.Users
{
    public interface IUserService
    {
        Task<UserResponse> CreateAsync(string accountId, string nickname, CancellationToken ct);
        Task<UserResponse> GetAsync(ulong userId, CancellationToken ct);
    }
}
