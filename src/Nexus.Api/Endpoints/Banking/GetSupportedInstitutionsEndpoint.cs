using FastEndpoints;
using Nexus.Application.Banking.DTOs;
using Nexus.Application.Banking.Queries;

namespace Nexus.Api.Endpoints.Banking;

public record GetSupportedInstitutionsRequest(string? Country);

public sealed class GetSupportedInstitutionsEndpoint : Endpoint<GetSupportedInstitutionsRequest, List<BankInstitutionDto>>
{
    private readonly GetSupportedInstitutionsQueryHandler _handler;

    public GetSupportedInstitutionsEndpoint(GetSupportedInstitutionsQueryHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/banking/institutions");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Obtiene las entidades bancarias soportadas para Open Banking";
            s.Description = "Lista las instituciones bancarias disponibles por país (por defecto España ES).";
        });
    }

    public override async Task HandleAsync(GetSupportedInstitutionsRequest req, CancellationToken ct)
    {
        var country = string.IsNullOrWhiteSpace(req.Country) ? "ES" : req.Country;
        var institutions = await _handler.HandleAsync(new GetSupportedInstitutionsQuery(country), ct);
        await Send.OkAsync(institutions, ct);
    }
}
