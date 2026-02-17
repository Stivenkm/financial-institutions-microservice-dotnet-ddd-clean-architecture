using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.AddLocalCode;

public sealed record AddLocalCodeCommand(
    FinancialInstitutionId Id,
    string Code,
    string CodeType
) : ICommand<FinancialInstitutionId>;