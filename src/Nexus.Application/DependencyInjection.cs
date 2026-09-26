using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Application.Auth.Commands.GoogleLogin;
using Nexus.Application.Auth.Commands.RefreshToken;
using Nexus.Application.Auth.Commands.SetupTotp;
using Nexus.Application.Auth.Commands.VerifyTotp;

namespace Nexus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Casos de uso de autenticación y seguridad
        services.AddScoped<GoogleLoginCommandHandler>();
        services.AddScoped<RefreshTokenCommandHandler>();
        services.AddScoped<SetupTotpCommandHandler>();
        services.AddScoped<VerifyTotpCommandHandler>();

        // Parsers de extractos bancarios (Strategy Pattern)
        services.AddSingleton<Nexus.Application.Statements.Parsers.IStatementParser, Nexus.Application.Statements.Parsers.BbvaStatementParser>();
        services.AddSingleton<Nexus.Application.Statements.Parsers.IStatementParser, Nexus.Application.Statements.Parsers.IngDirectStatementParser>();
        services.AddSingleton<Nexus.Application.Statements.Parsers.StatementParserFactory>();

        // Casos de uso de cuentas y extractos bancarios (001-raw-statements-storage)
        services.AddScoped<Nexus.Application.Statements.Commands.CreateBankAccount.CreateBankAccountCommandHandler>();
        services.AddScoped<Nexus.Application.Statements.Commands.PreviewStatement.PreviewStatementCommandHandler>();
        services.AddScoped<Nexus.Application.Statements.Commands.ConfirmStatementImport.ConfirmStatementImportCommandHandler>();
        services.AddScoped<Nexus.Application.Statements.Commands.RollbackStatementImport.RollbackStatementImportCommandHandler>();
        services.AddScoped<Nexus.Application.Statements.Queries.GetBankAccounts.GetBankAccountsQueryHandler>();
        services.AddScoped<Nexus.Application.Statements.Queries.GetAccountTransactions.GetAccountTransactionsQueryHandler>();

        return services;
    }
}
