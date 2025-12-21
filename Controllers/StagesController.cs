using Microsoft.AspNetCore.Mvc;
using MiniServerProject.Application.Stages;
using MiniServerProject.Controllers.Request;

namespace MiniServerProject.Controllers
{
    [ApiController]
    [Route("stages")]
    public sealed class StagesController : ControllerBase
    {
        private readonly IStageService _stageService;

        public StagesController(IStageService stageService)
        {
            _stageService = stageService;
        }

        [HttpPost("{stageId}/enter")]
        public async Task<IActionResult> Enter(string stageId, [FromBody] EnterStageRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(stageId))
                return BadRequest("stageId is required.");
            if (request.UserId == 0)
                return BadRequest("userId is required.");
            if (string.IsNullOrWhiteSpace(request.RequestId))
                return BadRequest("requestId is required.");

            var resp = await _stageService.EnterAsync(stageId, request, ct);
            return Ok(resp);
        }

        [HttpPost("{stageId}/clear")]
        public async Task<IActionResult> Clear(string stageId, [FromBody] ClearStageRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(stageId))
                return BadRequest("stageId is required.");
            if (request.UserId == 0)
                return BadRequest("userId is required.");
            if (string.IsNullOrWhiteSpace(request.RequestId))
                return BadRequest("requestId is required.");

            var resp = await _stageService.ClearAsync(stageId, request, ct);
            return Ok(resp);
        }

        [HttpPost("{stageId}/give-up")]
        public async Task<IActionResult> GiveUp(string stageId, [FromBody] GiveUpStageRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(stageId))
                return BadRequest("stageId is required.");
            if (request.UserId == 0)
                return BadRequest("userId is required.");
            if (string.IsNullOrWhiteSpace(request.RequestId))
                return BadRequest("requestId is required.");

            var resp = await _stageService.GiveUpAsync(stageId, request, ct);
            return Ok(resp);
        }
    }
}
