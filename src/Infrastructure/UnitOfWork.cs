namespace Intec.Banking.FinancialInstitutions.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}