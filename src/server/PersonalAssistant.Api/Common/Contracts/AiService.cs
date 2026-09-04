namespace PersonalAssistant.Api.Common.Contracts.AiService;

using PersonalAssistant.Api.Domain.Enums;

public record ChatMessageItem(
    MessageRole Role,
    string Content
);

public record AiServiceRequest(
    Guid SessionId,
    string UserMessage,
    List<ChatMessageItem> History,
    string? CurrentSummary,
    string Task,
    string Persona,
    string Tone,
    bool ShouldSummarize
);

public record AiServiceResponse(
    string Answer
);