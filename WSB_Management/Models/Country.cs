using System.Globalization;
using System.Text;

namespace WSB_Management.Models;

public class Country
{
    private string _shorttxt = "";
    private string _longtxt = "";

    public long Id { get; set; }

    public string Shorttxt
    {
        get => _shorttxt;
        set { _shorttxt = value.ToUpper() ?? ""; UpdateFlagPath(); }
    }

    public string Longtxt
    {
        get => _longtxt;
        set { _longtxt = value ?? ""; }
    }

    /// <summary>
    /// Relativer Web-Pfad, z. B. "/flags/flagge-litauen.png"
    /// </summary>
    public string FlagPath { get; set; } = "";

    /// <summary>
    /// Setzt FlagPath auf Basis von Longtxt/Shorttxt.
    /// </summary>
    public void UpdateFlagPath()
    {
        const string folder = "flags";
        const string prefix = "flagge-";

        var basis = string.IsNullOrWhiteSpace(Longtxt) ? Shorttxt.ToLower() : Longtxt.ToLower();
        if (string.IsNullOrWhiteSpace(basis))
        {
            FlagPath = "";
            return;
        }

        var fileName = $"{prefix}{Slug(basis)}.png";
        FlagPath = $"/{folder}/{fileName}";
    }

    public static string Slug(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "unbenannt";

        var s = input.Trim().ToLowerInvariant()
            .Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss");

        var normalized = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        s = sb.ToString().Normalize(NormalizationForm.FormC);

        var chars = s.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        s = new string(chars);
        while (s.Contains("--")) s = s.Replace("--", "-");
        return s.Trim('-');
    }
}
