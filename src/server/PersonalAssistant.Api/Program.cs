using FluentValidation;

using PersonalAssistant.Api.Features;
using PersonalAssistant.Api.Features.Chat;
using PersonalAssistant.Api.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRO DE SERVICIOS (DI)

// Infraestructura (BD, LLM, Clientes HTTP)
builder.Services.AddInfrastructure(builder.Configuration);

// Features (Lógica de negocio y Handlers)
builder.Services.AddFeatures();

// Validadores (FluentValidation nativo)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
// builder.Services.AddScoped<IValidator<SendMessageRequest>, SendMessageValidator>();

// Herramientas de desarrollo
builder.Services.AddOpenApi();


// 2. PIPELINE HTTP Y MAPEO DE RUTAS
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 3. Registro de Endpoints
app.MapChatEndpoints();

app.Run();