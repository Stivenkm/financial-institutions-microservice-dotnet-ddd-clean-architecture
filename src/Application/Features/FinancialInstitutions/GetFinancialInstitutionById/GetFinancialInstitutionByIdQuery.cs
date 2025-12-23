using Intec.Banking.FinancialInstitutions.Application.DTOs;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Intec.Banking.FinancialInstitutions.Primitives;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.GetFinancialInstitutionById;

public record GetFinancialInstitutionByIdQuery(FinancialInstitutionId Id) : IQuery<FinancialInstitutionDto?>;
