namespace PersonalAssistant.Api.Features.Chat;

using Microsoft.AspNetCore.Http.HttpResults;
using PersonalAssistant.Common.Extensions;

using PersonalAssistant.Api.Features.Chat.SendMessage;
using PersonalAssistant.Api.Features.Chat.GetChatHistory;

public static class ChatEndpoints
{
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat")
                       .WithTags("Chat");

        // POST /api/chat/send
        group.MapPost("/send", async Task<Ok<SendMessageResponse>> (
            SendMessageRequest request,
            SendMessageHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(request, ct);
            return TypedResults.Ok(response);
        })
        .WithValidation<SendMessageRequest>()
        .WithName("SendMessage")
        .WithSummary("Envía un mensaje al chat, procesa la inferencia con el LLM y persiste la interacción.");

        //GET /api/char/history/{sessionId}
        group.MapGet("/history/{sessionId:guid}", async Task<Results<Ok<GetChatHistoryResponse>, NotFound>> (
            Guid sessionId,
            GetChatHistoryHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(sessionId, ct);
            return response is not null
                ? TypedResults.Ok(response)
                : TypedResults.NotFound();
        })
        .WithName("GetChatHistory")
        .WithSummary("Obtiene el historial completo de mensajes de una sesión ordenados cronológicamente.");

        return app;
    }
}