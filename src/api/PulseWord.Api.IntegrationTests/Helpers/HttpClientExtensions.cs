using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PulseWord.Api.IntegrationTests.Helpers;

public static class HttpClientExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static async Task<T?> ReadAsJsonAsync<T>(this HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, DefaultOptions);
    }

    public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient client, string url, T value)
    {
        var content = new StringContent(JsonSerializer.Serialize(value, DefaultOptions), Encoding.UTF8, "application/json");
        return client.PostAsync(url, content);
    }

    public static Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient client, string url, T value)
    {
        var content = new StringContent(JsonSerializer.Serialize(value, DefaultOptions), Encoding.UTF8, "application/json");
        return client.PutAsync(url, content);
    }

    public static HttpClient WithTestAuth(this HttpClient client, string userId = "00000000-0000-0000-0000-000000000001", string userName = "test-user")
    {
        // Note: The TestAuthHandler will pick up these if we pass them as headers or if we configure the factory to use them.
        // For simplicity, we are just using the default ones configured in TestAuthHandler.
        // If we wanted to vary them per request, we'd need to adjust the handler to look at headers.
        return client;
    }
}
