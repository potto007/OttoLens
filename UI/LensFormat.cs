using System.Globalization;
using OttoLens.Model;

namespace OttoLens;

/// Formatters shared by readers and the panel. Plain English, no vanilla tokens except through
/// Name(), which localizes a vanilla item or piece token.
public static class LensFormat
{
    private static readonly Dictionary<string, string> NameCache = new(StringComparer.Ordinal);
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    /// Absolute short time: m:ss under 10 minutes, Xm Ys under an hour, Xh Ym above,
    /// "Ready" at or below zero. Never a percent.
    public static string Time(float seconds)
    {
        if (seconds <= 0f || float.IsNaN(seconds))
        {
            return "Ready";
        }

        int total = Mathf.CeilToInt(seconds);
        if (total < 600)
        {
            return string.Format(Culture, "{0}:{1:00}", total / 60, total % 60);
        }

        if (total < 3600)
        {
            return string.Format(Culture, "{0}m {1}s", total / 60, total % 60);
        }

        return string.Format(Culture, "{0}h {1:00}m", total / 3600, total % 3600 / 60);
    }

    /// Time plus an optional game day figure, for fuel and grow times that outlast an hour.
    /// Honors the ShowDays knob; falls back to Time() when no EnvMan exists.
    public static string TimeWithDays(float seconds)
    {
        string clock = Time(seconds);
        if (seconds <= 0f || !OttoLensPlugin.ShowDays.Value)
        {
            return clock;
        }

        EnvMan? env = EnvMan.instance;
        if (env == null || env.m_dayLengthSec <= 0)
        {
            return clock;
        }

        // m_dayLengthSec is a long in 1.0.7; cast before dividing.
        float days = seconds / (float)env.m_dayLengthSec;
        if (days < 0.5f)
        {
            return clock;
        }

        return string.Format(Culture, "{0} ({1:0.0} days)", clock, days);
    }

    /// "12/20" style capacity text.
    public static string Count(int current, int max)
        => max > 0 ? string.Format(Culture, "{0}/{1}", current, max) : current.ToString(Culture);

    public static string Count(float current, int max)
        => Count(Mathf.CeilToInt(current), max);

    /// "87%" from a 0..1 fraction.
    public static string Percent(float fraction)
        => string.Format(Culture, "{0}%", Mathf.RoundToInt(Mathf.Clamp01(fraction) * 100f));

    /// "Free 13" footer for anything with slot capacity.
    public static string Free(int freeSlots)
        => string.Format(Culture, "Free {0}", freeSlots);

    /// Health style ramp: high is Good, low is Bad.
    public static LensColor Ramp(float fraction)
        => fraction > 0.5f ? LensColor.Good : fraction > 0.15f ? LensColor.Warn : LensColor.Bad;

    /// Fill style ramp for capacity: near full is Warn, full is Bad, else Dim.
    public static LensColor FillRamp(float fraction)
        => fraction >= 1f ? LensColor.Bad : fraction >= 0.9f ? LensColor.Warn : LensColor.Dim;

    /// Localized name for a vanilla token such as "$item_wood", cached per token. Plain
    /// strings pass through. On any failure the token is shown with the $ stripped.
    public static string Name(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return "";
        }

        if (NameCache.TryGetValue(token!, out string cached))
        {
            return cached;
        }

        string result;
        try
        {
            Localization? loc = Localization.instance;
            result = loc != null ? loc.Localize(token!) : StripToken(token!);
            if (result.Length == 0 || result.Contains("$") || result.Contains("MISSING KEY"))
            {
                result = StripToken(token!);
            }
        }
        catch (Exception)
        {
            result = StripToken(token!);
        }

        NameCache[token!] = result;
        return result;
    }

    /// Title case for the panel title row; vanilla names are already display text.
    public static string Upper(string text) => text.ToUpperInvariant();

    internal static void ClearCaches() => NameCache.Clear();

    private static string StripToken(string token)
    {
        string s = token.Replace("$", "");
        return s.Replace('_', ' ');
    }
}
