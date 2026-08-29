namespace PersonalAssistant.Api.Features;

using PersonalAssistant.Api.Features.Chat.SendMessage;

public static class FeaturesDependencyInjection
{
    public static IServiceCollection AddFeatures(
       this IServiceCollection services)
    {
        services.AddScoped<SendMessageHandler>();
        // services.AddScoped<GetHistoryHandler>();
        // services.AddScoped<CreateSessionHandler>();

        return services;
    }
}