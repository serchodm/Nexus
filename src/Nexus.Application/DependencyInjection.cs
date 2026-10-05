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

        // Casos de uso de categorización, reglas y desglose de gastos (002-transaction-categorization)
        services.AddScoped<Nexus.Application.Categories.Queries.GetCategoryTree.GetCategoryTreeQueryHandler>();
        services.AddScoped<Nexus.Application.Categories.Commands.CreateCategory.CreateCategoryCommandHandler>();
        services.AddScoped<Nexus.Application.Categories.Commands.UpdateCategory.UpdateCategoryCommandHandler>();
        services.AddScoped<Nexus.Application.Categories.Commands.DeleteCategory.DeleteCategoryCommandHandler>();

        services.AddScoped<Nexus.Application.Rules.Queries.GetRules.GetRulesQueryHandler>();
        services.AddScoped<Nexus.Application.Rules.Commands.CreateCategorizationRule.CreateCategorizationRuleCommandHandler>();
        services.AddScoped<Nexus.Application.Rules.Commands.UpdateCategorizationRule.UpdateCategorizationRuleCommandHandler>();
        services.AddScoped<Nexus.Application.Rules.Commands.DeleteCategorizationRule.DeleteCategorizationRuleCommandHandler>();
        services.AddScoped<Nexus.Application.Rules.Commands.ApplyRulesRetroactive.ApplyRulesRetroactiveCommandHandler>();

        services.AddScoped<Nexus.Application.Transactions.Queries.GetTransactions.GetTransactionsQueryHandler>();
        services.AddScoped<Nexus.Application.Transactions.Queries.GetTransactionById.GetTransactionByIdQueryHandler>();
        services.AddScoped<Nexus.Application.Transactions.Commands.CategorizeTransaction.CategorizeTransactionCommandHandler>();
        services.AddScoped<Nexus.Application.Transactions.Commands.SplitTransaction.SplitTransactionCommandHandler>();
        services.AddScoped<Nexus.Application.Transactions.Commands.ResetTransactionSplits.ResetTransactionSplitsCommandHandler>();
        services.AddScoped<Nexus.Application.Transactions.Commands.LinkTransaction.LinkTransactionCommandHandler>();
        services.AddScoped<Nexus.Application.Transactions.Commands.UpdateTransactionNature.UpdateTransactionNatureCommandHandler>();

        // Casos de uso de conectores de comercio y reconciliación (003-commerce-connectors-and-reconciliation)
        services.AddScoped<Nexus.Application.Commerce.Commands.RunReconciliationCommandHandler>();
        services.AddScoped<Nexus.Application.Commerce.Commands.ImportAmazonOrderReportCommandHandler>();
        services.AddScoped<Nexus.Application.Commerce.Commands.ImportPayPalActivityCommandHandler>();
        services.AddScoped<Nexus.Application.Commerce.Commands.AcceptReconciliationCommandHandler>();
        services.AddScoped<Nexus.Application.Commerce.Commands.RejectReconciliationCommandHandler>();
        services.AddScoped<Nexus.Application.Commerce.Commands.ConnectPayPalCommandHandler>();
        services.AddScoped<Nexus.Application.Commerce.Commands.SyncPayPalApiCommandHandler>();
        services.AddScoped<Nexus.Application.Commerce.Queries.GetPendingReconciliationsQueryHandler>();
        services.AddScoped<Nexus.Application.Commerce.Queries.GetCommerceOrdersQueryHandler>();

        return services;
    }
}
