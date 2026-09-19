using Backend.Domain.Enums;
using Backend.Domain.Rules;

namespace Backend.Tests;

public sealed class ControlNumberRulesTests
{
    [Fact]
    public void Normalize_Trims_Whitespace()
    {
        Assert.Equal("abc123", ControlNumberRules.Normalize("  abc123  "));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_Rejects_Missing(string? value)
    {
        Assert.Throws<ArgumentException>(() => ControlNumberRules.Normalize(value));
    }

    [Fact]
    public void Normalize_Does_Not_Enforce_Institutional_Format()
    {
        // RN-02 canceled: any non-empty text is accepted, no year/prefix/length rule.
        Assert.Equal("XYZ-1", ControlNumberRules.Normalize("XYZ-1"));
        Assert.Equal("0013", ControlNumberRules.Normalize("0013"));
    }
}
