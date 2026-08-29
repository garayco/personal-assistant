namespace PersonalAssistant.Api.Features.Chat.SendMessage;

using FluentValidation;


public class SendMessageValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("El SessionId es obligatorio.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("El mensaje no puede estar vacío.")
            .MaximumLength(4000)
            .WithMessage("El mensaje no puede superar los 4000 caracteres.");
    }
}