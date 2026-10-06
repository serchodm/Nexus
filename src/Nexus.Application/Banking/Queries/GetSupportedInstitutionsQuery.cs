using Nexus.Application.Banking.DTOs;
using Nexus.Domain.Interfaces;

namespace Nexus.Application.Banking.Queries;

public record GetSupportedInstitutionsQuery(string Country = "ES");

public class GetSupportedInstitutionsQueryHandler
{
    private readonly IEnableBankingClient _enableBankingClient;

    public GetSupportedInstitutionsQueryHandler(IEnableBankingClient enableBankingClient)
    {
        _enableBankingClient = enableBankingClient;
    }

    public async Task<List<BankInstitutionDto>> HandleAsync(GetSupportedInstitutionsQuery query, CancellationToken ct = default)
    {
        var institutions = await _enableBankingClient.GetSupportedInstitutionsAsync(query.Country, ct);

        return institutions.Select(i => new BankInstitutionDto(
            i.Code,
            i.Name,
            i.LogoUrl,
            i.Country
        )).ToList();
    }
}
