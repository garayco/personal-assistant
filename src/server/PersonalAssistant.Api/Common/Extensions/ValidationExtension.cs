
namespace PersonalAssistant.Common.Extensions;

public static class ValidationExtensions
{
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) where T : class
    {
        return builder.AddEndpointFilter<ValidationFilter<T>>()
                      .ProducesValidationProblem();
    }
}