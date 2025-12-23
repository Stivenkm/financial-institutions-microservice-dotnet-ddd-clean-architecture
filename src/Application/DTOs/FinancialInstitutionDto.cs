namespace Intec.Banking.FinancialInstitutions.Application.DTOs;

public record FinancialInstitutionDto(
    Guid Id,
    string OfficialName,
    string? TradeName,
    string Country);
