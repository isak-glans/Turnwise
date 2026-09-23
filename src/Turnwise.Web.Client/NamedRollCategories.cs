using Turnwise.Domain.Enums;

namespace Turnwise.Web.Client;

/// <summary>Icons for the optional Weapon/Magic/Skill/Ability tag on a named roll, used on its row and its combat log entries.</summary>
public static class NamedRollCategories
{
    private const string DefaultIcon = "bi-dice-6-fill";

    /// <summary>Weapon has no fitting Bootstrap Icon, so it's rendered as this emoji instead of a "bi-*" class - see <see cref="IsEmoji"/>.</summary>
    public const string WeaponEmoji = "⚔️";

    /// <summary>True when <see cref="GetIcon"/> doesn't apply and <see cref="WeaponEmoji"/> should be rendered as text instead of a "bi-*" icon class.</summary>
    public static bool IsEmoji(NamedRollCategory? category) => category == NamedRollCategory.Weapon;

    public static string GetIcon(NamedRollCategory? category) => category switch
    {
        NamedRollCategory.Magic => "bi-magic",
        NamedRollCategory.Skill => "bi-mortarboard-fill",
        NamedRollCategory.Ability => "bi-bar-chart-line-fill",
        _ => DefaultIcon
    };
}
