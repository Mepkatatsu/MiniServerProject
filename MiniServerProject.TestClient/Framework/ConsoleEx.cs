using MiniServerProject.Shared.Responses;
using MiniServerProject.Shared.Tables;
using MiniServerProject.TestClient.App;

namespace MiniServerProject.TestClient.Framework
{
    public static class ConsoleEx
    {
        public static void WriteClientContextFull(ClientContext client)
        {
            Console.WriteLine("================================");
            Console.WriteLine($"계정: {client.AccountId}");
            Console.WriteLine($"UID: {client.UserId}");
            Console.WriteLine($"닉네임: {client.Nickname}");
            Console.WriteLine($"레벨: {client.Level}");
            Console.WriteLine($"스태미너: {client.Stamina}");
            Console.WriteLine($"골드: {client.Gold}");
            Console.WriteLine($"경험치: {client.Exp}");
            Console.WriteLine("================================");
        }

        public static void WriteClientContextSimple(ClientContext client)
        {
            Console.WriteLine("================================");
            Console.WriteLine($"닉네임: {client.Nickname}");
            Console.WriteLine($"레벨: {client.Level}");
            Console.WriteLine($"스태미너: {client.Stamina}");
            Console.WriteLine($"골드: {client.Gold}");
            Console.WriteLine($"경험치: {client.Exp}");
            Console.WriteLine("================================");
        }

        public static void WriteSimpleClientContextWithStageName(ClientContext client)
        {
            Console.WriteLine("================================");
            Console.WriteLine($"닉네임: {client.Nickname}");
            Console.WriteLine($"레벨: {client.Level}");
            Console.WriteLine($"스태미너: {client.Stamina}");
            Console.WriteLine($"골드: {client.Gold}");
            Console.WriteLine($"경험치: {client.Exp}");
            Console.WriteLine($"현재 스테이지: {client.GetStageName()}");
            Console.WriteLine("================================");
        }

        public static void WriteClearStageResponse(ClearStageResponse response)
        {
            Console.WriteLine("================================");
            Console.WriteLine($"스테이지 클리어!");
            Console.WriteLine($"획득 골드: {response.GainGold}");
            Console.WriteLine($"획득 경험치: {response.GainExp}");
            Console.WriteLine("================================");
            Console.ReadLine();
        }

        public static void WriteGiveUpStageResponse(GiveUpStageResponse response)
        {
            Console.WriteLine("================================");
            Console.WriteLine($"스테이지 클리어 실패...");
            Console.WriteLine($"반환 스태미너: {response.RefundStamina}");
            Console.WriteLine("================================");
            Console.ReadLine();
        }

        public static void WriteTitle(string title)
        {
            Console.WriteLine("================================");
            Console.WriteLine(title);
            Console.WriteLine("================================");
        }

        public static void WriteStageList()
        {
            Console.WriteLine("================================");
            Console.WriteLine("스테이지 목록");
            Console.WriteLine("================================");

            var stageNum = 1;

            foreach (var stageName in TableHolder.GetTable<StageTable>().GetAllKeys())
            {
                Console.WriteLine($"{stageNum}) {stageName}");
            }
        }

        public static void WriteLineWithWait(string message)
        {
            Console.WriteLine(message);
            Console.ReadLine(); // 유저가 에러 메세지를 확인할 수 있도록 입력 대기
        }

        public static void WriteErrorLineWithWait(string message)
        {
            Console.Error.WriteLine(message);
            Console.ReadLine(); // 유저가 에러 메세지를 확인할 수 있도록 입력 대기
        }
    }
}
