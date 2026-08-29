namespace PersonalAssistant.Api.Features.Chat.SendMessage;

using PersonalAssistant.Api.Domain.Enums;

public record SendMessageResponse(
    Guid Id,
    Guid SessionId,
    MessageRole Role,
    string Content,
    DateTime CreatedAt
);