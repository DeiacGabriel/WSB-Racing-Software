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
    public void UpdateFlagPath(string fileNameBase = "")
    {
        const string folder = "flags";
        const string prefix = "flagge-";

        // Map bekannte 2-Buchstaben-Codes auf vorhandene deutsche Dateinamen
        var code = Shorttxt.Trim().ToUpperInvariant();
        var map = new Dictionary<string, string>
        {
            ["AT"] = "oesterreich",
            ["DE"] = "deutschland",
            ["CH"] = "schweiz",
            ["IT"] = "italien",
            ["FR"] = "frankreich",
            ["ES"] = "spanien",
            ["PT"] = "portugal",
            ["NL"] = "niederlande",
            ["BE"] = "belgien",
            ["LU"] = "luxemburg",
            ["DK"] = "daenemark",
            ["SE"] = "schweden",
            ["NO"] = "norwegen",
            ["FI"] = "finnland",
            ["IE"] = "irland",
            ["GB"] = "grossbritanien",
            ["TR"] = "tuerkei",
            ["CZ"] = "tschechien",
            ["SK"] = "slowakei",
            ["SI"] = "slowenien",
            ["HR"] = "kroatien",
            ["RS"] = "serbien",
            ["BA"] = "bosnien",
            ["MK"] = "mazedonien",
            ["HU"] = "ungarn",
            ["RO"] = "rumaenien",
            ["BG"] = "bulgarien",
            ["PL"] = "polen",
            ["UA"] = "ukraine",
            ["BY"] = "weissrussland",
            ["RU"] = "russland",
            ["GR"] = "griechenland",
            ["LT"] = "litauen",
            ["LV"] = "lettland",
            ["EE"] = "estland",
            ["IS"] = "island",
            ["LI"] = "liechtenstein",
            ["MD"] = "moldawien",
            ["MC"] = "monaco",
            ["ME"] = "montenegro",
            ["SM"] = "san-marino",
            ["CY"] = "republik-zypern",
            ["VA"] = "vatikan",
            ["MT"] = "malta",
            ["KZ"] = "kasachstan",
            ["SA"] = "saudi",
            ["EU"] = "eu"
        };

        string basis = "";
        if (!string.IsNullOrWhiteSpace(fileNameBase))
        {
            basis = fileNameBase;
        }
        else if (!string.IsNullOrWhiteSpace(code) && map.TryGetValue(code, out var mapped))
        {
            basis = mapped;
        }
        else
        {
            basis = string.IsNullOrWhiteSpace(Longtxt) ? Shorttxt.ToLowerInvariant() : Longtxt.ToLowerInvariant();
        }

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
