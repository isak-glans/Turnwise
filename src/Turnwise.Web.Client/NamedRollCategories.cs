using Turnwise.Domain.Enums;

namespace Turnwise.Web.Client;

/// <summary>Icons for the optional Weapon/Magic/Skill tag on a named roll, used on its row and its combat log entries.</summary>
public static class NamedRollCategories
{
    private const string DefaultIcon = "bi-dice-6-fill";

    public static string GetIcon(NamedRollCategory? category) => category switch
    {
        NamedRollCategory.Weapon => "bi-hammer",
        NamedRollCategory.Magic => "bi-magic",
        NamedRollCategory.Skill => "bi-mortarboard-fill",
        _ => DefaultIcon
    };
}
