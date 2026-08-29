using FluentValidation;

namespace PersonalAssistant.Common.Extensions;

public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
{
    private readonly IValidator<T> _validator = validator;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.Arguments.OfType<T>().FirstOrDefault() is not { } argument)
        {
            return TypedResults.BadRequest("El cuerpo de la solicitud no puede ser nulo.");
        }

        var validationResult = await _validator.ValidateAsync(argument, context.HttpContext.RequestAborted);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        return await next(context);
    }
}