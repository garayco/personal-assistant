namespace PersonalAssistant.Api.Infrastructure.Llm;

using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Pgvector;

using PersonalAssistant.Api.Common.Contracts;

public interface ILlmClient
{
    Task<AiServiceResponse> GenerateResponseAsync(
        AiServiceRequest aiRequest,
        CancellationToken cancellationToken = default);

    Task<SummaryResponse> GenerateSummaryAsync(
        SummaryRequest summaryRequest,
        CancellationToken cancellationToken = default);

    Task<Vector> GetEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default);
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

    public async Task<AiServiceResponse> GenerateResponseAsync(
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

        var result = await response.Content.ReadFromJsonAsync<AiServiceResponse>(
            options: AiServiceJsonOptions,
            cancellationToken: ct);

        return result ?? throw new InvalidOperationException("El servicio de IA devolvió una respuesta vacía.");
    }

    public async Task<SummaryResponse> GenerateSummaryAsync(
        SummaryRequest summaryRequest,
        CancellationToken ct)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "summary",
            summaryRequest,
            AiServiceJsonOptions,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Error generando resumen ({response.StatusCode}): {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<SummaryResponse>(
            options: AiServiceJsonOptions,
            cancellationToken: ct);

        return result ?? new SummaryResponse("Sin resumen generado.", []);
    }

    public async Task<Vector> GetEmbeddingAsync(
        string text,
        CancellationToken ct = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "embeddings",
            new EmbeddingRequest(text),
            AiServiceJsonOptions,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Error obteniendo embedding ({response.StatusCode}): {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(
            options: AiServiceJsonOptions,
            cancellationToken: ct);

        if (result is null || result.Embedding.Length == 0)
        {
            throw new InvalidOperationException("El servicio de IA devolvió un embedding vacío.");
        }

        return new Vector(result.Embedding);
    }
}