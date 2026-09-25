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

        return services;
    }
}
