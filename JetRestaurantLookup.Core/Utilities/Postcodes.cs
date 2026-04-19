using System.Text.RegularExpressions;

namespace JetRestaurantLookup.Core.Utilities;

public static class Postcodes
{
    // BS7666 postcode validation — any one match is sufficient.
    // Official UK Gov Source => https://assets.publishing.service.gov.uk/media/632b07338fa8f53cb77ef6b8/WS02_LRS_Web_Services_Interface_Specification_v6.4.pdf
    private static readonly Regex[] _bs7666Patterns =
    [
        new(@"^[A-Z]{1,2}[0-9R][0-9A-Z]? ?[0-9][ABDEFGHJLNPQRSTUWXYZ]{2}$", RegexOptions.Compiled),
        new(@"^BFPO ?[0-9]{1,4}$",                                             RegexOptions.Compiled),
        new(@"^([AC-FHKNPRTV-Y]\d{2}|D6W)? ?[0-9AC-FHKNPRTV-Y]{4}$",      RegexOptions.Compiled),
    ];

    private static string Normalize(string raw) =>
        raw.Replace(" ", "").ToUpperInvariant();

    /// <summary>
    /// Returns false if the input fails to match the official uk gov regex for a postcode.
    /// Returning true does not guarantee that it is a real postcode, just that the format is valid. 
    /// </summary>
    public static bool IsValid(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var normalised = Normalize(raw);
        return _bs7666Patterns.Any(p => p.IsMatch(normalised));
    }

    /// <summary>
    /// Returns the outward code (e.g. "EC4M" from "EC4M 7RF").
    /// </summary>
    public static string Outcode(string raw)
    {
        var normalised = Normalize(raw);
        return normalised[..^3];
    }

    /// <summary>
    /// Returns the inward code (e.g. "7RF" from "EC4M 7RF").
    /// </summary>
    public static string Incode(string raw)
    {
        var normalised = Normalize(raw);
        return normalised[^3..];
    }

    /// <summary>
    /// Returns the postcode with a space (e.g. "EC4M 7RF").
    /// </summary>
    public static string WithSpace(string raw) =>
        $"{Outcode(raw)} {Incode(raw)}";

    /// <summary>
    /// Returns the postcode without a space (e.g. "EC4M7RF").
    /// </summary>
    public static string WithoutSpace(string raw) =>
        Normalize(raw);
}
