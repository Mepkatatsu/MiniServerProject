namespace MiniServerProject.Application
{
    public static class IdempotencyKeyFactory
    {
        public static string CreateUser(string accountId)
            => $"idem:users:create:{accountId}";

        public static string StageEnter(ulong userId, string stageId, string requestId)
            => $"idem:stages:enter:{userId}:{stageId}:{requestId}";

        public static string StageClear(ulong userId, string stageId, string requestId)
            => $"idem:stages:clear:{userId}:{stageId}:{requestId}";

        public static string StageGiveUp(ulong userId, string stageId, string requestId)
            => $"idem:stages:give-up:{userId}:{stageId}:{requestId}";
    }
}
