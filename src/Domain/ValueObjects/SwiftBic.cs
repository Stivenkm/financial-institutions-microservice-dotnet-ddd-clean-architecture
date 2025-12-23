using Intec.Banking.FinancialInstitutions.Primitives;
using System.Text.RegularExpressions;

namespace Intec.Banking.FinancialInstitutions.Domain.ValueObjects;

public sealed class SwiftBic : ValueObject
{
    private static readonly Regex SwiftBicRegex = new(@"^[A-Z]{6}[A-Z0-9]{2}([A-Z0-9]{3})?$", RegexOptions.Compiled);

    public string Code { get; }

    private SwiftBic(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("SWIFT/BIC code cannot be empty.", nameof(code));

        var normalizedCode = code.Replace(" ", "").ToUpperInvariant();

        if (!SwiftBicRegex.IsMatch(normalizedCode))
            throw new ArgumentException("Invalid SWIFT/BIC code format. Must be 8 or 11 characters.", nameof(code));

        Code = normalizedCode;
    }

    public static SwiftBic Create(string code) => new(code);

    public bool IsTestCode => Code.Length == 8 && Code[7] == '0';

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
    }

    public override string ToString() => Code;
}
