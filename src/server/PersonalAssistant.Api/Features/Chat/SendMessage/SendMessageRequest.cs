namespace PersonalAssistant.Api.Features.Chat.SendMessage;

public record SendMessageRequest(
    Guid? SessionId,
    string Message
);

