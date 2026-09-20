namespace Turnwise.Domain.Enums;

/// <summary>Optional tag for a named roll, shown as an icon on the roll row and on its combat log entries. A fixed set, same reasoning as <see cref="CombatantCategory"/> - consistent color/icon coding beats free text.</summary>
public enum NamedRollCategory
{
    Weapon,
    Magic,
    Skill
}
