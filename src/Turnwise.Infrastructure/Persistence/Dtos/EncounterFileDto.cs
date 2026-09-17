namespace Turnwise.Infrastructure.Persistence.Dtos;

public sealed class CombatLogEntryDto
{
    public Guid Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid? CombatantId { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>Root object for a saved encounter (*.turnwise-encounter.json): round, turn order and every combatant's full sheet.</summary>
public sealed class EncounterFileDto
{
    public int SchemaVersion { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Round { get; set; }
    public Guid? ActiveCombatantId { get; set; }
    public List<CombatantDto> Combatants { get; set; } = [];
    public List<CombatLogEntryDto> Log { get; set; } = [];

    /// <summary>Shared, deduplicated portrait pool: image id (content hash) -> base64. Combatants reference an entry via <see cref="CombatantDto.PortraitImageId"/> instead of embedding their own copy, since several combatants (e.g. a pack of identical goblins) often share the same art.</summary>
    public Dictionary<string, string> Images { get; set; } = [];
}
