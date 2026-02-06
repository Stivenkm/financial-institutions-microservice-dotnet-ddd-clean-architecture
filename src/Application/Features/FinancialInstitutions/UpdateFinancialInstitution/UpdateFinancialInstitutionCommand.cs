using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinanciialInstitutions.Application.Features.FinnacialInstitutions.UpdateFinancialIntituion;

public record UpdateFinancialInstitutionCommand(
    FinancialInstitutionId Id,
    string OfficialName,
    string? TradeName,
    string CountryCode,
    string TaxIdValue,
    string? SwiftBicCode
) : ICommand<FinancialInstitutionId>;
