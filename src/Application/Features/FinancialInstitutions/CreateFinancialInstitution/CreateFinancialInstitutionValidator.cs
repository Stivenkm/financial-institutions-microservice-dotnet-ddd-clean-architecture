using FluentValidation;

namespace Intec.Banking.FinancialInstitutions.Application.Features.FinancialInstitutions.CreateFinancialInstitution;

public sealed class CreateFinancialInstitutionValidator : AbstractValidator<CreateFinancialInstitutionCommand>
{
    public CreateFinancialInstitutionValidator()
    {
        RuleFor(x => x.OfficialName)
            .NotEmpty()
            .WithMessage("Official name is required.")
            .MaximumLength(200)
            .WithMessage("Official name cannot exceed 200 characters.");

        RuleFor(x => x.TradeName)
            .MaximumLength(200)
            .WithMessage("Trade name cannot exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.TradeName));

        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .WithMessage("Country code is required.")
            .Length(2, 3)
            .WithMessage("Country code must be 2 or 3 characters (ISO 3166).");

        RuleFor(x => x.TaxIdValue)
            .NotEmpty()
            .WithMessage("Tax ID is required.");

        RuleFor(x => x.SwiftBicCode)
            .Matches(@"^[A-Z]{6}[A-Z0-9]{2}([A-Z0-9]{3})?$")
            .WithMessage("SWIFT/BIC must be 8 or 11 uppercase characters: 6 letters + 2 alphanumeric + optional 3 alphanumeric.")
            .When(x => !string.IsNullOrWhiteSpace(x.SwiftBicCode));
    }
}