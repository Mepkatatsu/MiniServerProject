using System.Net.Http.Json;

namespace MiniServerProject.TestClient.Api
{
    public sealed class ApiClient
    {
        private readonly HttpClient _http;

        public ApiClient(string baseUrl)
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public async Task<T?> PostAsync<TRequest, T>(string url, TRequest body, bool skipError = false)
        {
            var response = await _http.PostAsJsonAsync(url, body);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }

            if (!skipError)
                Console.Error.WriteLine($"PostAsync failed. url:{url}, StatusCode: {response.StatusCode}");

            return default;
        }

        public async Task<T?> GetAsync<T>(string url, bool skipError = false)
        {
            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }

            if (!skipError)
                Console.Error.WriteLine($"GetAsync failed. url:{url}, StatusCode: {response.StatusCode}");

            return default;
        }
    }
}
