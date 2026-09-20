namespace Turnwise.Application.Common;

/// <summary>
/// Schema versions for the two file formats. Files older than the minimum supported version
/// are rejected outright - there is no automatic migration in the initial version.
/// </summary>
public static class SchemaVersions
{
    /// <summary>
    /// Bumped for: dropping Combatant.MaxHpLocked/InitiativeLocked (the lock buttons were replaced
    /// by an edit-mode toggle, a UI-only concept now), adding NamedRoll.Category, adding
    /// Combatant.TemporaryHp, and adding Counter.Category. All are safe for files at
    /// <see cref="MinSupportedCharacterSchemaVersion"/>: dropped fields are simply ignored on
    /// load, and new fields come back empty/zero.
    /// </summary>
    public const int CurrentCharacterSchemaVersion = 6;
    public const int MinSupportedCharacterSchemaVersion = 3;

    /// <summary>Bumped alongside <see cref="CurrentCharacterSchemaVersion"/>, same reasoning (encounter files embed full combatant sheets and their own log, which gained CombatLogEntry.RollCategory).</summary>
    public const int CurrentEncounterSchemaVersion = 8;
    public const int MinSupportedEncounterSchemaVersion = 5;

    /// <summary>Portrait images embedded as base64 are capped to keep character files a reasonable size.</summary>
    public const int MaxPortraitBase64Length = 2_000_000; // ~1.5 MB decoded
}
