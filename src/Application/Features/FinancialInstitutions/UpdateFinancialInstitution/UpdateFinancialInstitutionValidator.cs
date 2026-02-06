using FluentValidation;
using Intec.Banking.FinanciialInstitutions.Application.Features.FinnacialInstitutions.UpdateFinancialIntituion;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.UpdateFinancialInstitution;

public sealed class UpdateFinancialInstitutionValidator : AbstractValidator<UpdateFinancialInstitutionCommand>
{
    public UpdateFinancialInstitutionValidator()
    {
        RuleFor(x => x.Id)
            .NotNull();

        RuleFor(x => x.OfficialName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.TradeName)
            .MaximumLength(200);

        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .Length(2, 3);

        RuleFor(x => x.TaxIdValue)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.SwiftBicCode)
            .MaximumLength(11);
    }
}
