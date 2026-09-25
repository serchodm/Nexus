using FastEndpoints;
using Nexus.Application.Common.Interfaces;
using Nexus.Domain.Common;
using Nexus.Domain.Repositories;

namespace Nexus.Api.Endpoints.Auth;

public sealed class LogoutEndpoint : EndpointWithoutRequest
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutEndpoint(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenGenerator tokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenGenerator = tokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public override void Configure()
    {
        Post("/auth/logout");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Cierra la sesión del usuario";
            s.Description = "Revoca el Refresh Token en la base de datos y elimina la Cookie HttpOnly del navegador.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (HttpContext.Request.Cookies.TryGetValue("nexus_refresh_token", out var rawToken)
            && !string.IsNullOrWhiteSpace(rawToken))
        {
            var hash = _tokenGenerator.HashToken(rawToken);
            var token = await _refreshTokenRepository.GetByTokenHashAsync(hash, ct);
            if (token != null && !token.IsRevoked)
            {
                token.RevokedAt = DateTimeOffset.UtcNow;
                await _unitOfWork.SaveChangesAsync(ct);
            }
        }

        HttpContext.Response.Cookies.Delete("nexus_refresh_token", new CookieOptions
        {
            Path = "/api/auth"
        });

        await Send.NoContentAsync(ct);
    }
}
