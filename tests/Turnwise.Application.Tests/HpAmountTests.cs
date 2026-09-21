using Turnwise.Application.Common;
using Xunit;

namespace Turnwise.Application.Tests;

public class HpAmountTests
{
    [Theory]
    [InlineData("7", 7)]
    [InlineData("  12  ", 12)]
    [InlineData("+7", 7)]
    [InlineData("-14", 14)]
    [InlineData("99999999999", int.MaxValue)]
    [InlineData("-99999999999", int.MaxValue)]
    public void TryParse_AcceptsWholeNumbersAndIgnoresTheSign(string text, int expected)
    {
        Assert.True(HpAmount.TryParse(text, out var amount));
        Assert.Equal(expected, amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("0")]
    [InlineData("-0")]
    [InlineData("99999999999999999999")]
    public void TryParse_RejectsEmptyNonNumericZeroAndOverflowingInput(string? text)
    {
        Assert.False(HpAmount.TryParse(text, out _));
    }
}
