namespace PersonalAssistant.Api.Features;

using PersonalAssistant.Api.Features.Chat.SendMessage;
using PersonalAssistant.Api.Features.Chat.GetChatHistory;

public static class FeaturesDependencyInjection
{
    public static IServiceCollection AddFeatures(
       this IServiceCollection services)
    {
        services.AddScoped<SendMessageHandler>();
        services.AddScoped<GetChatHistoryHandler>();

        return services;
    }
}