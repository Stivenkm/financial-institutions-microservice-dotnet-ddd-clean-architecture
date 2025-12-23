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
        {
            _context.FinancialInstitutions.Remove(institution);
        }
    }
}