using MiniServerProject.Shared.Requests;
using MiniServerProject.Shared.Responses;
using MiniServerProject.Shared.Tables;
using MiniServerProject.TestClient.Framework;

namespace MiniServerProject.TestClient.App
{
    public sealed class ClientApp
    {
        private readonly ClientContext _ctx = new();

        public async Task RunAsync()
        {
            ConsoleEx.WriteTitle("MiniServer Test Client");

            _ctx.LoadClientData();
            _ctx.BuildApi();

            while (_ctx.State != AppState.Exit)
            {
                Console.Clear();
                switch (_ctx.State)
                {
                    case AppState.Boot:
                        await BootAsync();
                        break;

                    case AppState.Login:
                        await LoginAsync();
                        break;

                    case AppState.CreateUser:
                        await CreateUserAsync();
                        break;

                    case AppState.Lobby:
                        await LobbyAsync();
                        break;

                    case AppState.EnterStage:
                        await EnterStageAsync();
                        break;

                    case AppState.InStage:
                        await InStageAsync();
                        break;

                    case AppState.ClearStage:
                        await ClearStageAsync();
                        break;

                    case AppState.GiveUpStage:
                        await GiveUpStageAsync();
                        break;

                    case AppState.Logout:
                        await LogoutAsync();
                        break;

                    default:
                        ConsoleEx.WriteErrorLineWithWait("현재 지원되지 않는 기능입니다.");
                        _ctx.State = AppState.Exit;
                        break;
                }
            }

            _ctx.SaveClientData();
            Console.WriteLine("Bye!");
        }

        private Task BootAsync()
        {
            Console.Write($"BaseUrl [{_ctx.BaseUrl}]: ");
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                _ctx.BaseUrl = input;

            _ctx.BuildApi();
            _ctx.State = AppState.Login;
            return Task.CompletedTask;
        }

        private async Task LoginAsync()
        {
            string? accountId = _ctx.AccountId;

            while (string.IsNullOrWhiteSpace(accountId))
            {
                Console.Write("AccountId 입력: ");

                accountId = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(accountId))
                    ConsoleEx.WriteErrorLineWithWait("AccountId 입력이 비정상적입니다. 다시 입력해주세요.");
            }

            var response = await _ctx.Api.GetAsync<UserResponse>($"/users/{accountId}", true);

            if (response == null)
            {
                _ctx.SetAccountId(accountId);
                _ctx.State = AppState.CreateUser;
            }
            else
            {
                _ctx.InitByUserResponse(accountId, response);
                _ctx.State = AppState.Lobby;
            }
        }

        private async Task CreateUserAsync()
        {
            var accountId = _ctx.AccountId;

            if (string.IsNullOrWhiteSpace(accountId))
            {
                ConsoleEx.WriteErrorLineWithWait("AccountId가 비정상적입니다. 다시 로그인해주세요.");
                _ctx.State = AppState.Login;
                return;
            }

            Console.WriteLine("새로운 계정을 생성합니다.");

            string? nickname = string.Empty;
            while (string.IsNullOrWhiteSpace(nickname))
            {
                Console.Write("닉네임 입력: ");
                nickname = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(nickname))
                    ConsoleEx.WriteErrorLineWithWait("nickname 입력이 비정상적입니다. 다시 입력해주세요.");
            }

            var request = new CreateUserRequest(accountId, nickname);
            var response = await _ctx.Api.PostAsync<CreateUserRequest, UserResponse>($"/users", request, false);

            if (response == null)
            {
                ConsoleEx.WriteErrorLineWithWait("로그인에 실패했습니다. 다시 시도해주세요.");
                return;
            }

            _ctx.InitByUserResponse(accountId, response);
            _ctx.State = AppState.Lobby;
        }

        private Task LobbyAsync()
        {
            if (!string.IsNullOrWhiteSpace(_ctx.CurrentStageId))
            {
                ConsoleEx.WriteLineWithWait($"진행중인 스테이지가 있습니다. 스테이지 이름:{_ctx.GetStageName()}");
                _ctx.State = AppState.InStage;
                return Task.CompletedTask;
            }

            ConsoleEx.WriteClientContextFull(_ctx);
            Console.WriteLine();
            Console.WriteLine("1) 스테이지 진입");
            Console.WriteLine("2) 로그아웃");
            Console.WriteLine("3) 종료");
            Console.Write("> ");

            var key = Console.ReadLine();
            _ctx.State = key switch
            {
                "1" => AppState.EnterStage,
                "2" => AppState.Logout,
                "3" => AppState.Exit,
                _ => _ctx.State
            };

            return Task.CompletedTask;
        }

        private async Task EnterStageAsync()
        {
            ConsoleEx.WriteClientContextSimple(_ctx);
            ConsoleEx.WriteStageList();
            Console.WriteLine();
            Console.WriteLine("입장을 원하시는 스테이지 이름을 입력해주세요.");
            Console.WriteLine("0) 로비로 돌아가기");
            Console.Write("> ");

            var key = Console.ReadLine();

            if (string.IsNullOrEmpty(key))
            {
                ConsoleEx.WriteErrorLineWithWait("입력이 잘못되었습니다. 다시 시도해주세요.");
                return;
            }

            if (string.Equals(key, "0"))
            {
                _ctx.State = AppState.Lobby;
                return;
            }

            var stageData = TableHolder.GetTable<StageTable>().Get(key);

            if (stageData == null)
            {
                ConsoleEx.WriteErrorLineWithWait("스테이지를 찾지 못했습니다. 다시 시도해주세요.");
                return;
            }

            var request = new EnterStageRequest(_ctx.UserId, _ctx.GetRequestId());
            var response = await _ctx.Api.PostAsync<EnterStageRequest, EnterStageResponse>($"/stages/{key}/enter", request);

            if (response == null)
            {
                ConsoleEx.WriteErrorLineWithWait("스테이지 입장에 실패했습니다. 다시 시도해주세요.");
                return;
            }

            _ctx.SetCurrentStageId(response.StageId);
            _ctx.SetStamina(response.AfterStamina);

            _ctx.State = AppState.InStage;
        }

        private Task InStageAsync()
        {
            ConsoleEx.WriteSimpleClientContextWithStageName(_ctx);
            Console.WriteLine();
            Console.WriteLine("1) 스테이지 클리어");
            Console.WriteLine("2) 스테이지 포기");
            Console.Write("> ");

            var key = Console.ReadLine();

            _ctx.State = key switch
            {
                "1" => AppState.ClearStage,
                "2" => AppState.GiveUpStage,
                _ => _ctx.State
            };

            return Task.CompletedTask;
        }

        private async Task ClearStageAsync()
        {
            var request = new ClearStageRequest(_ctx.UserId, _ctx.GetRequestId());
            var response = await _ctx.Api.PostAsync<ClearStageRequest, ClearStageResponse>($"/stages/{_ctx.CurrentStageId}/clear", request);

            if (response == null)
            {
                ConsoleEx.WriteErrorLineWithWait("스테이지 클리어에 실패했습니다. 다시 시도해주세요.");
                _ctx.State = AppState.InStage;
                return;
            }

            ConsoleEx.WriteClearStageResponse(response);
            _ctx.ClearCurrentStageId();
            _ctx.SetGold(response.AfterGold);
            _ctx.SetExp(response.AfterExp);

            _ctx.State = AppState.Lobby;
        }

        private async Task GiveUpStageAsync()
        {
            var request = new GiveUpStageRequest(_ctx.UserId, _ctx.GetRequestId());
            var response = await _ctx.Api.PostAsync<GiveUpStageRequest, GiveUpStageResponse>($"/stages/{_ctx.CurrentStageId}/give-up", request);

            if (response == null)
            {
                ConsoleEx.WriteErrorLineWithWait("스테이지 포기에 실패했습니다. 다시 시도해주세요.");
                _ctx.State = AppState.InStage;
                return;
            }

            ConsoleEx.WriteGiveUpStageResponse(response);
            _ctx.ClearCurrentStageId();
            _ctx.SetStamina(response.AfterStamina);

            _ctx.State = AppState.Lobby;
        }

        private Task LogoutAsync()
        {
            _ctx.ClearAccountId();

            _ctx.State = AppState.Login;

            return Task.CompletedTask;
        }
    }
}
