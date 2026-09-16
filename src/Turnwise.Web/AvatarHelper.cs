namespace Turnwise.Web;

/// <summary>Deterministic color + initials for a combatant's avatar placeholder when no portrait image is set.</summary>
public static class AvatarHelper
{
    private static readonly string[] Palette =
    [
        "#2563eb", "#7c3aed", "#059669", "#d97706", "#dc2626", "#0891b2", "#c026d3", "#4f46e5"
    ];

    public static string ColorFor(Guid id)
    {
        var index = (int)((uint)id.GetHashCode() % (uint)Palette.Length);
        return Palette[index];
    }

    public static string InitialsFor(string name)
    {
        var parts = name
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.Any(char.IsLetter))
            .ToArray();

        return parts.Length switch
        {
            0 => "?",
            1 => parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant(),
            _ => $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant()
        };
    }
}
