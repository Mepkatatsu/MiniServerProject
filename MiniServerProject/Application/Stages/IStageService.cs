using MiniServerProject.Shared.Responses;

namespace MiniServerProject.Application.Stages
{
    public interface IStageService
    {
        Task<EnterStageResponse> EnterAsync(ulong userId, string requestId, string stageId, CancellationToken ct, int testDelayMs = 0);
        Task<ClearStageResponse> ClearAsync(ulong userId, string requestId, string stageId, CancellationToken ct, int testDelayMs = 0);
        Task<GiveUpStageResponse> GiveUpAsync(ulong userId, string requestId, string stageId, CancellationToken ct, int testDelayMs = 0);
    }
}
