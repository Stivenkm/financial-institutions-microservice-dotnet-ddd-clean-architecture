using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.CreateFinancialInstitution;

public record CreateFinancialInstitutionCommand(
    string OfficialName,
    string? TradeName,
    string CountryCode,
    string TaxIdValue,
    string? SwiftBicCode) : ICommand<FinancialInstitutionId>;
