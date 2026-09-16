namespace Turnwise.Infrastructure.Persistence.Dtos;

/// <summary>Root object for a saved character template (*.turnwise-character.json).</summary>
public sealed class CharacterFileDto
{
    public int SchemaVersion { get; set; }
    public CombatantDto Character { get; set; } = new();
}
