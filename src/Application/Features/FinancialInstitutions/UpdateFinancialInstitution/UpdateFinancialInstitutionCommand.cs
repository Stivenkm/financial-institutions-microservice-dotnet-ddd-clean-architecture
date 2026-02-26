using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.UpdateFinancialInstituion;

public record UpdateFinancialInstitutionCommand(
    FinancialInstitutionId Id,
    string OfficialName,
    string? TradeName,
    string CountryCode,
    string TaxIdValue,
    string? SwiftBicCode,
    long? OriginalVersion
) : ICommand<FinancialInstitutionId>;
