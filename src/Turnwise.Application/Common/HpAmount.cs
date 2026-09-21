using System.Globalization;

namespace Turnwise.Application.Common;

/// <summary>
/// Parses the free-text amount typed next to the Damage/Heal buttons. The button carries the
/// direction, so the amount is a plain positive number - a stray leading +/- (left over from
/// the old signed input) is tolerated and ignored rather than flipping the direction.
/// </summary>
public static class HpAmount
{
    public static bool TryParse(string? text, out int amount)
    {
        amount = 0;
        if (!long.TryParse(text?.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value))
        {
            return false;
        }

        // Widen first so long.MinValue-style inputs can't overflow Math.Abs; anything beyond int range just clamps.
        var magnitude = value == long.MinValue ? long.MaxValue : Math.Abs(value);
        if (magnitude == 0)
        {
            return false;
        }

        amount = (int)Math.Min(magnitude, int.MaxValue);
        return true;
    }
}
