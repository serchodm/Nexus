using System.Security.Claims;
using FastEndpoints;
using FluentValidation;
using Nexus.Application.Statements.Commands.CreateBankAccount;
using Nexus.Application.Statements.DTOs;
using Nexus.Domain.Enums;

namespace Nexus.Api.Endpoints.Statements;

public record CreateBankAccountRequest(
    string Name,
    string? Iban,
    AccountType Type,
    HolderRole HolderRole,
    decimal? OwnershipPercentage,
    List<string>? LinkedCardsLast4
);

public sealed class CreateBankAccountValidator : Validator<CreateBankAccountRequest>
{
    public CreateBankAccountValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la cuenta es requerido.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.OwnershipPercentage)
            .InclusiveBetween(0.01m, 100.00m)
            .When(x => x.OwnershipPercentage.HasValue)
            .WithMessage("El porcentaje de titularidad debe estar entre 0.01% y 100.00%.");
    }
}

public sealed class CreateBankAccountEndpoint : Endpoint<CreateBankAccountRequest, BankAccountDto>
{
    private readonly CreateBankAccountCommandHandler _handler;

    public CreateBankAccountEndpoint(CreateBankAccountCommandHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/accounts");
        Summary(s =>
        {
            s.Summary = "Crea una nueva cuenta bancaria";
            s.Description = "Crea la cuenta asignando rol de titularidad y autodectando entidad bancaria mediante los 4 dígitos del IBAN.";
        });
    }

    public override async Task HandleAsync(CreateBankAccountRequest req, CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
        {
            ThrowError("Usuario no autenticado.", 401);
        }

        var command = new CreateBankAccountCommand(
            UserId: userId,
            Name: req.Name,
            Iban: req.Iban,
            Type: req.Type,
            HolderRole: req.HolderRole,
            OwnershipPercentage: req.OwnershipPercentage,
            LinkedCardsLast4: req.LinkedCardsLast4
        );

        var result = await _handler.HandleAsync(command, ct);

        if (!result.Success || result.Account == null)
        {
            ThrowError(result.ErrorMessage ?? "Error al crear la cuenta bancaria.", 400);
        }

        await Send.CreatedAtAsync<GetBankAccountsEndpoint>(null, result.Account, cancellation: ct);
    }
}
