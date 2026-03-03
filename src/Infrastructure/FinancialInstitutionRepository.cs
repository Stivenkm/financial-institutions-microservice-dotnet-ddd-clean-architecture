using Intec.Banking.FinancialInstitutions.Domain;
using Intec.Banking.FinancialInstitutions.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Intec.Banking.FinancialInstitutions.Infrastructure;

public class FinancialInstitutionRepository : IFinancialInstitutionRepository
{
    private readonly FinancialInstitutionsDbContext _context;

    public FinancialInstitutionRepository(FinancialInstitutionsDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialInstitution?> GetByIdAsync(FinancialInstitutionId id, CancellationToken ct = default)
    {
        return await _context.FinancialInstitutions
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<List<FinancialInstitution>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.FinancialInstitutions.ToListAsync(ct);
    }

    public async Task<List<FinancialInstitution>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        return await _context.FinancialInstitutions
            .AsNoTracking()
            .OrderBy(x => x.OfficialName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<List<FinancialInstitution>> SearchAsync(string? country, string? name, string? swiftBicCode,int page, int pageSize, CancellationToken ct = default)
    {
        var queryable = _context.FinancialInstitutions
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(country))
        {
            // HasConversion: pass the ValueObject — EF applies the converter correctly.
            // Comparing string directly causes InvalidCastException on parameter binding.
            var countryCode = CountryCode.Create(country.Trim().ToUpperInvariant());
            queryable = queryable.Where(x => x.Country == countryCode);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            var nameFilter = $"%{name.Trim()}%";
            queryable = queryable.Where(x =>
                EF.Functions.ILike(x.OfficialName, nameFilter) ||
                (x.TradeName != null && EF.Functions.ILike(x.TradeName, nameFilter)));
        }

        if (!string.IsNullOrWhiteSpace(swiftBicCode))
        {
            // HasConversion: pass the ValueObject — EF applies the converter correctly.
            // Comparing string directly causes InvalidCastException on parameter binding.
            var swift = SwiftBic.Create(swiftBicCode.Trim().ToUpperInvariant());
            queryable = queryable.Where(x => x.SwiftBic == swift);
        }

        return await queryable
            .OrderBy(x => x.OfficialName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task AddAsync(FinancialInstitution institution, CancellationToken ct = default)
    {
        await _context.FinancialInstitutions.AddAsync(institution, ct);
    }

    public async Task UpdateAsync(FinancialInstitution institution, CancellationToken ct = default)
    {
        _context.FinancialInstitutions.Update(institution);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(FinancialInstitutionId id, CancellationToken ct = default)
    {
        var institution = await GetByIdAsync(id, ct);
        if (institution != null)
            _context.FinancialInstitutions.Remove(institution);
    }
}