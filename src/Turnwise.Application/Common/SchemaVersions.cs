namespace Turnwise.Application.Common;

/// <summary>
/// Schema versions for the two file formats. Files older than the minimum supported version
/// are rejected outright - there is no automatic migration in the initial version.
/// </summary>
public static class SchemaVersions
{
    /// <summary>Bumped for the addition of Combatant.Category/Notes - purely additive, so files at <see cref="MinSupportedCharacterSchemaVersion"/> still load fine (the new fields just come back empty).</summary>
    public const int CurrentCharacterSchemaVersion = 4;
    public const int MinSupportedCharacterSchemaVersion = 3;

    /// <summary>Bumped alongside <see cref="CurrentCharacterSchemaVersion"/>, same reasoning (encounter files embed full combatant sheets).</summary>
    public const int CurrentEncounterSchemaVersion = 6;
    public const int MinSupportedEncounterSchemaVersion = 5;

    /// <summary>Portrait images embedded as base64 are capped to keep character files a reasonable size.</summary>
    public const int MaxPortraitBase64Length = 2_000_000; // ~1.5 MB decoded
}
