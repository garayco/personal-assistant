namespace PersonalAssistant.Api.Infrastructure;

using Microsoft.EntityFrameworkCore;
using PersonalAssistant.Api.Infrastructure.Database;
using PersonalAssistant.Api.Infrastructure.Llm;


public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Base de datos
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.UseVector();
            });
            options.UseSnakeCaseNamingConvention();
        }
);

        // 2. Configuración y Cliente LLM
        services.Configure<AiServiceOptions>(
            configuration.GetSection(AiServiceOptions.SectionName));

        services.AddHttpClient<ILlmClient, OpenAiLlmClient>();

        return services;
    }
}