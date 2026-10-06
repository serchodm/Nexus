using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nexus.Application.Common.Interfaces;
using Nexus.Domain.Common;
using Nexus.Domain.Repositories;
using Nexus.Infrastructure.Persistence;
using Nexus.Infrastructure.Persistence.Interceptors;
using Nexus.Infrastructure.Persistence.Repositories;
using Nexus.Infrastructure.Services;

namespace Nexus.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            })
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
        });

        // Repositorios y Unit of Work desacoplados
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<IStatementImportRepository, StatementImportRepository>();
        services.AddScoped<IBankTransactionRepository, BankTransactionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategorizationRuleRepository, CategorizationRuleRepository>();
        services.AddScoped<IExternalCommerceOrderRepository, ExternalCommerceOrderRepository>();
        services.AddScoped<ITransactionReconciliationRepository, TransactionReconciliationRepository>();
        services.AddScoped<IExternalCommerceConnectionRepository, ExternalCommerceConnectionRepository>();
        services.AddScoped<ISharedSpaceRepository, SharedSpaceRepository>();
        services.AddScoped<ISharedExpenseRepository, SharedExpenseRepository>();
        services.AddScoped<ISharedSettlementRepository, SharedSettlementRepository>();

        // Servicios de Seguridad y Autenticación (Fase 3)
        services.AddSingleton<ITokenGenerator, TokenGenerator>();
        services.AddSingleton<ITotpService, TotpService>();
        services.AddSingleton<IJwtProvider, JwtProvider>();
        services.AddSingleton<ISecretEncryptionService, AesGcmEncryptionService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddHttpClient<IPayPalApiClient, PayPalApiClient>();

        // Configuración de Autenticación JWT Bearer
        var jwtKey = configuration["Jwt:Key"] 
            ?? throw new InvalidOperationException("Falta la configuración de 'Jwt:Key'.");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "NexusApi";
        var jwtAudience = configuration["Jwt:Audience"] ?? "NexusFrontend";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}
