using MiniServerProject.Shared.Responses;

namespace MiniServerProject.Application.Stages
{
    public interface IStageService
    {
        Task<EnterStageResponse> EnterAsync(ulong userId, string requestId, string stageId, CancellationToken ct);
        Task<ClearStageResponse> ClearAsync(ulong userId, string requestId, string stageId, CancellationToken ct);
        Task<GiveUpStageResponse> GiveUpAsync(ulong userId, string requestId, string stageId, CancellationToken ct);
    }
}
