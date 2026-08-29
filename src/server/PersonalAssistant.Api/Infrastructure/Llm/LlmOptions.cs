namespace PersonalAssistant.Api.Infrastructure.Llm;

public class LlmOptions
{
    public const string SectionName = "Llm";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public double Temperature { get; set; } = 0.7;
}