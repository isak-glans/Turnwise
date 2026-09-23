using System.Text.Json.Serialization;

namespace Turnwise.Infrastructure.Persistence.Dtos;

/// <summary>Root object for a saved character template (*.turnwise-character.json).</summary>
public sealed class CharacterFileDto
{
    /// <summary>Format documentation for a human or AI reading the file - see <see cref="FileReadme"/>. Never read back on load.</summary>
    [JsonPropertyName("_readme")]
    public string Readme { get; set; } = FileReadme.ForCharacterFile;

    public int SchemaVersion { get; set; }
    public CombatantDto Character { get; set; } = new();
}
