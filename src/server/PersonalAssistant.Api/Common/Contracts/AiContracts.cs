namespace PersonalAssistant.Api.Common.Contracts;

using PersonalAssistant.Api.Domain.Enums;

public record ChatMessageItem(
    MessageRole Role,
    string Content
);

public record AiServiceRequest(
    Guid SessionId,
    string UserMessage,
    List<ChatMessageItem> History,
    string? CurrentSummary = null,
    List<string>? RelevantMemories = null,
    List<string>? ActiveHabits = null
);

public record AiServiceResponse(
    string Answer,
    int PromptTokens
);

public record SummaryRequest(
    Guid SessionId,
    List<ChatMessageItem> History,
    string? CurrentSummary,
    List<string>? ExistingFacts = null
);

public record UpdatedFactItem(
    int Id,
    string Updated
);

public record SummaryResponse(
    string Summary,
    List<string> NewFacts,
    List<UpdatedFactItem>? UpdatedFacts = null
);

public record EmbeddingRequest(
    string Text
);

public record EmbeddingResponse(
    float[] Embedding
);
