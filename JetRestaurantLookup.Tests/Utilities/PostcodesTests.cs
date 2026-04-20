using JetRestaurantLookup.Core.Utilities;

namespace JetRestaurantLookup.Tests.Utilities;

public class PostcodesTests
{

    // --- IsValid ---

    [Theory]
    [InlineData("EC4M7RF")]
    [InlineData("EC4M 7RF")]
    [InlineData("ec4m 7rf")]
    [InlineData("SW1A2AA")]
    [InlineData("W1A1AA")]
    [InlineData("BFPO1234")]
    public void IsValid_ValidFormats_ReturnsTrue(string raw)
    {
        Assert.True(Postcodes.IsValid(raw));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("HELLO")]
    [InlineData("12345")]
    [InlineData("NOTAPOSTCODE")]
    public void IsValid_InvalidFormats_ReturnsFalse(string? raw)
    {
        Assert.False(Postcodes.IsValid(raw));
    }

    // --- Outcode / Incode ---

    [Theory]
    [InlineData("EC4M7RF",  "EC4M", "7RF")]
    [InlineData("EC4M 7RF", "EC4M", "7RF")]
    [InlineData("SW1A2AA",  "SW1A", "2AA")]
    [InlineData("W1A1AA",   "W1A",  "1AA")]
    public void Outcode_And_Incode_SplitCorrectly(string raw, string expectedOut, string expectedIn)
    {
        Assert.Equal(expectedOut, Postcodes.Outcode(raw));
        Assert.Equal(expectedIn,  Postcodes.Incode(raw));
    }

    // --- WithoutSpace (normalisation) ---

    [Theory]
    [InlineData("EC4M 7RF", "EC4M7RF")]
    [InlineData("ec4m7rf",  "EC4M7RF")]
    [InlineData("ec4m 7rf", "EC4M7RF")]
    public void WithoutSpace_StripsSpacesAndUppercases(string raw, string expected)
    {
        Assert.Equal(expected, Postcodes.WithoutSpace(raw));
    }

    // --- WithSpace ---

    [Theory]
    [InlineData("EC4M7RF",  "EC4M 7RF")]
    [InlineData("ec4m 7rf", "EC4M 7RF")]
    [InlineData("SW1A2AA",  "SW1A 2AA")]
    public void WithSpace_ReturnsSpacedForm(string raw, string expected)
    {
        Assert.Equal(expected, Postcodes.WithSpace(raw));
    }
}
