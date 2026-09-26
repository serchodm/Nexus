using FluentValidation;

namespace Nexus.Application.Statements.Commands.CreateBankAccount;

public class CreateBankAccountCommandValidator : AbstractValidator<CreateBankAccountCommand>
{
    public CreateBankAccountCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la cuenta es requerido.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.OwnershipPercentage)
            .InclusiveBetween(0m, 100m).When(x => x.OwnershipPercentage.HasValue)
            .WithMessage("El porcentaje de titularidad debe estar entre 0 y 100.");
    }
}
