using Turnwise.Domain.Enums;

namespace Turnwise.Web.Client;

/// <summary>Icons for the optional Ammunition/Potions/Abilities tag on a counter, used on its row.</summary>
public static class CounterCategories
{
    private const string DefaultIcon = "bi-hexagon-fill";

    public static string GetIcon(CounterCategory? category) => category switch
    {
        CounterCategory.Ammunition => "bi-bullseye",
        CounterCategory.Potions => "bi-droplet-fill",
        CounterCategory.Abilities => "bi-lightning-fill",
        _ => DefaultIcon
    };
}
