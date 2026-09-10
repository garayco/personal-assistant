namespace PersonalAssistant.Api.Features;

using PersonalAssistant.Api.Features.Chat;

public static class FeaturesDependencyInjection
{
    public static IServiceCollection AddFeatures(this IServiceCollection services)
    {
        services.AddScoped<SendMessageHandler>();
        services.AddScoped<GetChatHistoryHandler>();
        return services;
    }
}