namespace Turnwise.Application.Common;

/// <summary>
/// Schema versions for the two file formats. Files older than the minimum supported version
/// are rejected outright - there is no automatic migration in the initial version.
/// </summary>
public static class SchemaVersions
{
    public const int CurrentCharacterSchemaVersion = 3;
    public const int MinSupportedCharacterSchemaVersion = 3;

    public const int CurrentEncounterSchemaVersion = 3;
    public const int MinSupportedEncounterSchemaVersion = 3;

    /// <summary>Portrait images embedded as base64 are capped to keep character files a reasonable size.</summary>
    public const int MaxPortraitBase64Length = 2_000_000; // ~1.5 MB decoded
}
