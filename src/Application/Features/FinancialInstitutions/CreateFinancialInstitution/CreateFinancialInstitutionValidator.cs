using FluentValidation;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.CreateFinancialInstitution;

public class CreateFinancialInstitutionValidator : AbstractValidator<CreateFinancialInstitutionCommand>
{
    public CreateFinancialInstitutionValidator()
    {
        RuleFor(x => x.OfficialName)
            .NotEmpty().WithMessage("Official name is required")
            .MaximumLength(200).WithMessage("Official name cannot exceed 200 characters");

        RuleFor(x => x.TradeName)
            .MaximumLength(200).WithMessage("Trade name cannot exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.TradeName));

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("Country code is required")
            .Length(2, 3).WithMessage("Country code must be 2 or 3 characters");

        RuleFor(x => x.TaxIdValue)
            .NotEmpty().WithMessage("Tax ID is required");

        RuleFor(x => x.SwiftBicCode)
            .Matches(@"^[A-Z]{6}[A-Z0-9]{2}([A-Z0-9]{3})?$")
            .WithMessage("Invalid SWIFT/BIC code format")
            .When(x => !string.IsNullOrWhiteSpace(x.SwiftBicCode));
    }
}
