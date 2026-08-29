namespace PersonalAssistant.Api.Infrastructure.Llm;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

public class OpenAiLlmClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly LlmOptions _options;

    public OpenAiLlmClient(HttpClient httpClient, IOptions<LlmOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }
    }

    public async Task<string> GenerateResponseAsync(string prompt, CancellationToken ct = default)
    {
        var requestPayload = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = "Eres un asistente personal conciso y eficiente." },
                new { role = "user", content = prompt }
            },
            temperature = _options.Temperature
        };

        var response = await _httpClient.PostAsJsonAsync("chat/completions", requestPayload, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Error LLM ({response.StatusCode}): {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: ct);
        return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "Sin respuesta del modelo.";
    }

    private record ChatCompletionResponse(List<ChoicePayload>? Choices);
    private record ChoicePayload(MessagePayload? Message);
    private record MessagePayload(string Content);
}