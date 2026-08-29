namespace PersonalAssistant.Api.Infrastructure.Llm;

public interface ILlmClient
{
    Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default);
}