namespace Intec.Banking.FinancialInstitutions.Application.Common;

/// <summary>
/// Encapsulates pagination normalization logic shared across query handlers.
/// Queries keep flat Page/PageSize parameters for endpoint binding compatibility.
/// </summary>
public readonly record struct PaginationParams
{
    public const int DefaultMinPage = 1;
    public const int DefaultMinPageSize = 1;
    public const int DefaultMaxPageSize = 100;

    public int Page { get; }
    public int PageSize { get; }

    public PaginationParams(int page, int pageSize,
        int minPage = DefaultMinPage,
        int minPageSize = DefaultMinPageSize,
        int maxPageSize = DefaultMaxPageSize)
    {
        Page = Math.Max(minPage, page);
        PageSize = Math.Clamp(pageSize, minPageSize, maxPageSize);
    }
}