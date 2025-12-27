using MiniServerProject.TestClient.App;
using System.Text.Json;

namespace MiniServerProject.TestClient.Framework
{
    public sealed class ClientDataHelper
    {
        private const string FileName = "testclient.json";

        public void Load(ClientContext ctx)
        {
            if (!File.Exists(FileName)) return;

            var json = File.ReadAllText(FileName);
            var data = JsonSerializer.Deserialize<ClientDataModel>(json);
            if (data == null) return;

            ctx.BaseUrl = data.BaseUrl ?? ctx.BaseUrl;
            ctx.SetAccountId(data.AccountId);
        }

        public void Save(ClientContext ctx)
        {
            var data = new ClientDataModel
            {
                BaseUrl = ctx.BaseUrl,
                AccountId = ctx.AccountId,
            };

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileName, json);
        }

        private sealed class ClientDataModel
        {
            public string? BaseUrl { get; set; }
            public string? AccountId { get; set; }
        }
    }
}
