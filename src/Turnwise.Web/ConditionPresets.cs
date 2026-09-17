namespace Turnwise.Web;

/// <summary>
/// Quick-pick condition suggestions and icons for the UI. Conditions themselves are free text
/// in the domain (any TRPG can use whatever names it wants) - these are just the classic D&D-style
/// conditions offered as autocomplete suggestions, since they come up in many systems.
/// </summary>
public static class ConditionPresets
{
    private static readonly (string Name, string Icon)[] Known =
    [
        ("Blinded", "bi-eye-slash-fill"),
        ("Charmed", "bi-heart-fill"),
        ("Dead", "bi-x-circle-fill"),
        ("Deafened", "bi-ear-fill"),
        ("Exhaustion", "bi-battery-half"),
        ("Frightened", "bi-emoji-dizzy-fill"),
        ("Grappled", "bi-hand-index-thumb-fill"),
        ("Incapacitated", "bi-x-octagon-fill"),
        ("Invisible", "bi-eye-slash"),
        ("Paralyzed", "bi-lightning-fill"),
        ("Petrified", "bi-gem"),
        ("Poisoned", "bi-droplet-fill"),
        ("Prone", "bi-arrow-down-circle-fill"),
        ("Restrained", "bi-link-45deg"),
        ("Stunned", "bi-stars"),
        ("Unconscious", "bi-moon-stars-fill")
    ];

    private const string DefaultIcon = "bi-exclamation-triangle-fill";

    public static IReadOnlyList<string> Names { get; } = Known.Select(k => k.Name).ToList();

    public static string GetIcon(string conditionName)
    {
        foreach (var (name, icon) in Known)
        {
            if (string.Equals(name, conditionName, StringComparison.OrdinalIgnoreCase))
            {
                return icon;
            }
        }

        return DefaultIcon;
    }
}
