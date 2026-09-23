namespace Turnwise.Infrastructure.Persistence;

/// <summary>
/// Plain-English format documentation embedded in every saved file (as "_readme"), so a GM can
/// hand the file to an AI assistant - along with a description of the character or monster they
/// want - and have it produce a new file Turnwise can load, without guessing at field shapes or
/// syntax. Purely informational: written on every save, never read back on load, so it's always
/// current regardless of what an older file (or a hand-edited one) happens to contain.
/// </summary>
internal static class FileReadme
{
    private const string CombatantFields =
        "MaxHp/CurrentHp/TemporaryHp are whole numbers (CurrentHp should be <= MaxHp; TemporaryHp is extra HP on top, 0 if unused). " +
        "Category is one of \"Enemy\", \"Ally\", \"RP\", or omitted/null. " +
        "InitiativeFormula and MaxHpFormula use dice notation \"NdX+Y\" or \"NdX-Y\" (e.g. \"1d20+4\", \"8d6+16\"); N (dice count) is 1-100, X (die size) is 2-1000, the modifier is -999 to 999; omit/null if there's no formula. " +
        "NamedRolls[].Formula uses that same dice notation. NamedRolls[].Category is \"Weapon\", \"Magic\", \"Skill\", \"Ability\", or omitted/null. " +
        "Counters[].Category is \"Ammunition\", \"Potions\", \"Abilities\", or omitted/null; Counters[].Current should be between 0 and Max. " +
        "Notes is Markdown text: **bold**, *italic*, \"- \" bullet lists, \"1. \" numbered lists, \"#\"/\"##\"/\"###\" headings, \"[label](https://...)\" links (only http/https/mailto links become clickable, others render as plain text). " +
        "Ids are GUIDs; any new, unique GUID works for a new item (e.g. \"00000000-0000-0000-0000-000000000001\") - they don't need to already exist anywhere. " +
        "PortraitBase64 (a base64-encoded image) is entirely optional and can be omitted.";

    public const string ForCharacterFile =
        "This is a Turnwise character template file (Turnwise is a TRPG combat tracker). " +
        "If you're an AI filling this out for a GM from a character description: " + CombatantFields;

    public const string ForEncounterFile =
        "This is a Turnwise encounter file (Turnwise is a TRPG combat tracker): Round is a whole number, " +
        "ActiveCombatantId is a GUID matching one of Combatants[].Id or null, Combatants is a list (each shaped " +
        "like a character file's \"Character\" object), and Log can usually just be left as an empty list []. " +
        "If you're an AI filling this out for a GM, for each combatant: " + CombatantFields +
        " Images can be left as an empty object {} - portraits are optional.";
}
