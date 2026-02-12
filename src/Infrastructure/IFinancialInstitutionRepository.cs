using Intec.Banking.FinancialInstitutions.Domain;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;

namespace Intec.Banking.FinancialInstitutions.Infrastructure;

public interface IFinancialInstitutionRepository
{
    Task<FinancialInstitution?> GetByIdAsync(FinancialInstitutionId id, CancellationToken ct = default);
    Task<List<FinancialInstitution>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(FinancialInstitution institution, CancellationToken ct = default);
    Task UpdateAsync(FinancialInstitution institution, CancellationToken ct = default);
    Task DeleteAsync(FinancialInstitutionId id, CancellationToken ct = default);
    Task<List<FinancialInstitution>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<List<FinancialInstitution>> SearchAsync(string? country,string? name,string? swiftBicCode, int page, int pageSize, CancellationToken ct = default);

}