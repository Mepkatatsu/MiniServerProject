using MiniServerProject.Controllers.Request;
using MiniServerProject.Controllers.Response;

namespace MiniServerProject.Application.Stages
{
    public interface IStageService
    {
        Task<EnterStageResponse> EnterAsync(string stageId, EnterStageRequest request, CancellationToken ct);
        Task<ClearStageResponse> ClearAsync(string stageId, ClearStageRequest request, CancellationToken ct);
        Task<GiveUpStageResponse> GiveUpAsync(string stageId, GiveUpStageRequest request, CancellationToken ct);
    }
}
