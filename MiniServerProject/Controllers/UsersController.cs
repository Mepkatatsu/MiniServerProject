using Microsoft.AspNetCore.Mvc;
using MiniServerProject.Application;
using MiniServerProject.Application.Users;
using MiniServerProject.Shared.Requests;

namespace MiniServerProject.Shared
{
    [ApiController]
    [Route("users")]
    public sealed class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // POST /users
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.AccountId))
                throw new DomainException(ErrorType.InvalidRequest, "AccountId is required.");
            if (string.IsNullOrWhiteSpace(request.Nickname))
                throw new DomainException(ErrorType.InvalidRequest, "Nickname is required.");

            var accountId = request.AccountId.Trim();
            var nickname = request.Nickname.Trim();

            var resp = await _userService.CreateAsync(accountId, nickname, ct);
            return Ok(resp);
        }

        // GET /users/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetById(ulong userId, CancellationToken ct)
        {
            var resp = await _userService.GetAsync(userId, ct);
            return Ok(resp);
        }
    }
}
