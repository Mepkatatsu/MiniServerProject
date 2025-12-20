using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniServerProject.Controllers.Request;
using MiniServerProject.Domain.Entities;
using MiniServerProject.Infrastructure.Persistence;

namespace MiniServerProject.Controllers
{
    [ApiController]
    [Route("users")]
    public sealed class UsersController : ControllerBase
    {
        private readonly GameDbContext _db;

        public UsersController(GameDbContext db)
        {
            _db = db;
        }

        // POST /users
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nickname))
                return BadRequest("Nickname is required.");

            var user = new User(request.Nickname);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = user.UserId },
                new
                {
                    user.UserId,
                    user.Nickname,
                    user.Level,
                    user.Stamina,
                    user.Gold,
                    user.Exp,
                    user.CreateDateTime,
                    user.LastStaminaUpdateTime,
                    user.CurrentStageId
                }
            );
        }

        // GET /users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(ulong id)
        {
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == id);

            if (user == null)
                return NotFound($"User not found. userId: {id}");

            return Ok(new
            {
                user.UserId,
                user.Nickname,
                user.Level,
                user.Stamina,
                user.Gold,
                user.Exp,
                user.CreateDateTime,
                user.LastStaminaUpdateTime,
                user.CurrentStageId
            });
        }
    }
}
