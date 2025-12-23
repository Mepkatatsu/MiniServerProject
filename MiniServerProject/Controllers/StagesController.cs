using Microsoft.AspNetCore.Mvc;
using MiniServerProject.Application;
using MiniServerProject.Application.Stages;
using MiniServerProject.Shared.Requests;

namespace MiniServerProject.Shared
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
                throw new DomainException(ErrorType.InvalidRequest, "stageId is required.");
            if (request.UserId == 0)
                throw new DomainException(ErrorType.InvalidRequest, "userId is required.");
            if (string.IsNullOrWhiteSpace(request.RequestId))
                throw new DomainException(ErrorType.InvalidRequest, "requestId is required.");

            var resp = await _stageService.EnterAsync(request.UserId, request.RequestId, stageId, ct);
            return Ok(resp);
        }

        [HttpPost("{stageId}/clear")]
        public async Task<IActionResult> Clear(string stageId, [FromBody] ClearStageRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(stageId))
                throw new DomainException(ErrorType.InvalidRequest, "stageId is required.");
            if (request.UserId == 0)
                throw new DomainException(ErrorType.InvalidRequest, "userId is required.");
            if (string.IsNullOrWhiteSpace(request.RequestId))
                throw new DomainException(ErrorType.InvalidRequest, "requestId is required.");

            var resp = await _stageService.ClearAsync(request.UserId, request.RequestId, stageId, ct);
            return Ok(resp);
        }

        [HttpPost("{stageId}/give-up")]
        public async Task<IActionResult> GiveUp(string stageId, [FromBody] GiveUpStageRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(stageId))
                throw new DomainException(ErrorType.InvalidRequest, "stageId is required.");
            if (request.UserId == 0)
                throw new DomainException(ErrorType.InvalidRequest, "userId is required.");
            if (string.IsNullOrWhiteSpace(request.RequestId))
                throw new DomainException(ErrorType.InvalidRequest, "requestId is required.");

            var resp = await _stageService.GiveUpAsync(request.UserId, request.RequestId, stageId, ct);
            return Ok(resp);
        }
    }
}
