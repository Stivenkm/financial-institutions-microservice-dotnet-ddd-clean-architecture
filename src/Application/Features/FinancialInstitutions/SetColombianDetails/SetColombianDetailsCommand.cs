using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.SetColombianDetails;

public sealed record SetColombianDetailsCommand(
    FinancialInstitutionId Id,
    string AchCode,
    string? SuperFinancialCode
) : ICommand<FinancialInstitutionId>;
