using Microsoft.AspNetCore.Mvc;
using MiniServerProject.Application.Stages;
using MiniServerProject.Application.Users;
using MiniServerProject.Controllers.Request;

namespace MiniServerProject.Controllers
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
                return BadRequest("AccountId is required.");
            if (string.IsNullOrWhiteSpace(request.Nickname))
                return BadRequest("Nickname is required.");

            var accountId = request.AccountId.Trim();
            var nickname = request.Nickname.Trim();

            try
            {
                var resp = await _userService.CreateAsync(accountId, nickname, ct);
                return Ok(resp);
            }
            catch (UserDomainException ex)
            {
                return ex.ToActionResult();
            }
        }

        // GET /users/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetById(ulong userId, CancellationToken ct)
        {
            try
            {
                var resp = await _userService.GetAsync(userId, ct);
                return Ok(resp);
            }
            catch (UserDomainException ex)
            {
                return ex.ToActionResult();
            }
        }
    }
}
