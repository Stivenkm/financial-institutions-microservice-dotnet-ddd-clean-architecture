

namespace Intec.Banking.FinancialInstitutions.Primitives;

public class Aggregate<TId> : Entity<TId>, IAggregate<TId>, IHaveDomainEvents, IAggregate,IHaveAudit,IHaveSoftDelete
{
    private readonly List<IDomainEvent> _domainEvents = new();

    // ────────────────────────────────────────────────────────────
    // IHaveCreator
    // ────────────────────────────────────────────────────────────

    public DateTime Created { get; private set; }
    public int? CreatedBy { get; private set; }

    // ────────────────────────────────────────────────────────────
    // IHaveAudit
    // ────────────────────────────────────────────────────────────

    public DateTime? LastModified { get; private set; }
    public int? LastModifiedBy { get; private set; }

    // ────────────────────────────────────────────────────────────
    // IHaveSoftDelete
    // ────────────────────────────────────────────────────────────

    public bool IsDeleted { get; set; }
    public DateTime? Deleted { get; set; }
    public int? DeletedBy { get; set; }

    // ────────────────────────────────────────────────────────────
    // IHaveAggregateVersion — optimistic concurrency
    // OriginalVersion: valor leído de la BD, usado en WHERE del UPDATE
    // Version: valor actual, se incrementa en cada SaveChanges
    // ────────────────────────────────────────────────────────────

    /// <summary>
    /// The version read from the store. Used by EF Core as concurrency token.
    /// EF generates: UPDATE ... WHERE Id = ? AND Version = OriginalVersion
    /// </summary>
    public long OriginalVersion { get; private set; }

    // ────────────────────────────────────────────────────────────
    // IHaveTenant — multi-tenancy
    // ────────────────────────────────────────────────────────────

    public Guid TenantId { get; private set; }


    // ────────────────────────────────────────────────────────────
    // AUDIT SETTERS — called by SaveChangesInterceptor
    // ────────────────────────────────────────────────────────────

    public void SetCreated(DateTime createdAt, int? createdBy)
    {
        Created = createdAt;
        CreatedBy = createdBy;
    }

    public void SetLastModified(DateTime modifiedAt, int? modifiedBy)
    {
        LastModified = modifiedAt;
        LastModifiedBy = modifiedBy;
    }

    public void SetDeleted(DateTime deletedAt, int? deletedBy)
    {
        IsDeleted = true;
        Deleted = deletedAt;
        DeletedBy = deletedBy;
    }

    public void IncrementVersion()
    {
        OriginalVersion++;
    }

    public void SetTenant(Guid tenantId)
    {
        TenantId = tenantId;
    }

    // ────────────────────────────────────────────────────────────
    // DOMAIN EVENTS
    // ────────────────────────────────────────────────────────────


    /// <summary>
    /// Add a domain event to the aggregate
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Does the aggregate have change that have not been committed to storage
    /// </summary>
    public bool HasUncommittedDomainEvents()
    {
        return _domainEvents.Any();
    }

    /// <summary>
    /// Gets a list of uncommitted events for this aggregate.
    /// </summary>
    public IReadOnlyList<IDomainEvent> GetUncommittedDomainEvents()
    {
        return _domainEvents.AsReadOnly();
    }

    /// <summary>
    /// Gets a list of uncommitted events for this aggregate, mark all events as committed.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DequeueUncommittedDomainEvents()
    {
        var events = _domainEvents.ToList().AsReadOnly();
        _domainEvents.Clear();
        return events;
    }

    /// <summary>
    /// Mark all changes (events) as committed, clears uncommitted changes.
    /// </summary>
    public void MarkUncommittedDomainEventAsCommitted()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Check specific rule for aggregate and throw an exception if rule is not satisfied.
    /// </summary>
    public void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }
}