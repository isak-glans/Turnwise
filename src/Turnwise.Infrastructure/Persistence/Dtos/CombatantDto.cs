namespace Turnwise.Infrastructure.Persistence.Dtos;

public sealed class NamedRollDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Formula { get; set; } = string.Empty;
}

public sealed class CounterDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Current { get; set; }
    public int Max { get; set; }
    public bool ShowBar { get; set; } = true;
}

public sealed class ConditionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>Wire format for a combatant's full character sheet, shared by character template files and encounter files.</summary>
public sealed class CombatantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    /// <summary>Inline portrait, used by character template files. Encounter files use <see cref="PortraitImageId"/> instead (see <see cref="EncounterFileDto.Images"/>).</summary>
    public string? PortraitBase64 { get; set; }

    /// <summary>Reference into an encounter file's shared, deduplicated image pool. Never set for character template files.</summary>
    public string? PortraitImageId { get; set; }

    public int MaxHp { get; set; }
    public bool MaxHpLocked { get; set; }
    public int CurrentHp { get; set; }
    public string? InitiativeFormula { get; set; }
    public int? Initiative { get; set; }
    public bool InitiativeLocked { get; set; }
    public List<NamedRollDto> NamedRolls { get; set; } = [];
    public List<CounterDto> Counters { get; set; } = [];
    public List<ConditionDto> Conditions { get; set; } = [];
}
