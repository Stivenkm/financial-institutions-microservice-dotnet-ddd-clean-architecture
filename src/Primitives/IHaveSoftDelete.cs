
namespace Intec.Banking.FinancialInstitutions.Primitives;

public interface IHaveSoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? Deleted { get;set; }
    int? DeletedBy { get;set; }
}