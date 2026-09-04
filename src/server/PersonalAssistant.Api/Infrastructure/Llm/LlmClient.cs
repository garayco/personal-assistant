namespace PersonalAssistant.Api.Infrastructure.Llm;

using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

using PersonalAssistant.Api.Common.Contracts.AiService;


public interface ILlmClient
{
    Task<string> GenerateResponseAsync(AiServiceRequest aiRequest, CancellationToken cancellationToken = default);
}

public class AiServiceOptions
{
    public const string SectionName = "AiService";
    public string BaseUrl { get; set; } = string.Empty;
}


public class OpenAiLlmClient : ILlmClient
{
    private static readonly JsonSerializerOptions AiServiceJsonOptions = new(JsonSerializerDefaults.Web);

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
        using var response = await _httpClient.PostAsJsonAsync(
            "chat",
            aiRequest,
            AiServiceJsonOptions,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Error LLM ({response.StatusCode}): {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<AiServiceResponse>(cancellationToken: ct);

        return string.IsNullOrWhiteSpace(result?.Answer)
        ? "Sin respuesta del modelo."
        : result.Answer;
    }
}