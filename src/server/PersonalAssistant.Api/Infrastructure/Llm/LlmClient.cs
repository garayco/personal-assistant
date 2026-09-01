namespace PersonalAssistant.Api.Infrastructure.Llm;

using System.Net.Http.Json;
using Microsoft.Extensions.Options;

using PersonalAssistant.Api.Common.Contracts.AiService;


public interface ILlmClient
{
    Task<string> GenerateResponseAsync(AiServiceRequest aiRequest, CancellationToken cancellationToken = default);
    Task<string> SummarizeAsync(SummarizeRequest summarizeRequest, CancellationToken cancellationToken = default);

}

public class AiServiceOptions
{
    public const string sectionName = "AiService";
    public string BaseUrl { get; set; } = string.Empty;
}


public class OpenAiLlmClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly AiServiceOptions _options;

    public OpenAiLlmClient(HttpClient httpClient, IOptions<AiServiceOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<string> GenerateResponseAsync(
        AiServiceRequest aiRequest,
        CancellationToken ct)
    {
        var response = await _httpClient.PostAsJsonAsync("chat", aiRequest, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Error LLM ({response.StatusCode}): {error}");
        }

        string? result = await response.Content.ReadFromJsonAsync<string>(cancellationToken: ct);

        return result ?? "Sin respuesta del modelo.";
    }

    public Task<string> SummarizeAsync(SummarizeRequest summarizeRequest, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}