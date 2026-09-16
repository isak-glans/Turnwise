using Turnwise.Domain.Entities;
using Turnwise.Domain.Enums;
using Turnwise.Domain.ValueObjects;
using Turnwise.Infrastructure.Persistence.Dtos;

namespace Turnwise.Infrastructure.Persistence;

internal static class CombatantMapper
{
    public static CombatantDto ToDto(Combatant combatant) => new()
    {
        Id = combatant.Id,
        Name = combatant.Name,
        Type = combatant.Type.ToString(),
        PortraitBase64 = combatant.PortraitBase64,
        MaxHp = combatant.MaxHp,
        MaxHpLocked = combatant.MaxHpLocked,
        CurrentHp = combatant.CurrentHp,
        InitiativeFormula = combatant.InitiativeFormula,
        Initiative = combatant.Initiative,
        InitiativeLocked = combatant.InitiativeLocked,
        NamedRolls = combatant.NamedRolls
            .Select(r => new NamedRollDto { Id = r.Id, Name = r.Name, Formula = r.Formula.ToString() })
            .ToList(),
        Counters = combatant.Counters
            .Select(c => new CounterDto { Id = c.Id, Name = c.Name, Current = c.Current, Max = c.Max, ShowBar = c.ShowBar })
            .ToList(),
        Conditions = combatant.Conditions
            .Select(c => new ConditionDto { Id = c.Id, Name = c.Name })
            .ToList()
    };

    public static Combatant ToDomain(CombatantDto dto)
    {
        if (!Enum.TryParse<CombatantType>(dto.Type, out var type))
        {
            throw new FormatException($"Unknown combatant type '{dto.Type}'.");
        }

        var combatant = Combatant.Restore(
            dto.Id,
            dto.Name,
            type,
            dto.MaxHp,
            dto.CurrentHp,
            dto.MaxHpLocked,
            dto.PortraitBase64,
            dto.InitiativeFormula,
            dto.Initiative,
            dto.InitiativeLocked);

        foreach (var roll in dto.NamedRolls)
        {
            combatant.AddNamedRoll(roll.Name, DiceFormula.Parse(roll.Formula), roll.Id);
        }

        foreach (var counter in dto.Counters)
        {
            combatant.AddCounter(counter.Name, counter.Current, counter.Max, counter.ShowBar, counter.Id);
        }

        foreach (var condition in dto.Conditions)
        {
            combatant.AddCondition(condition.Name, condition.Id);
        }

        return combatant;
    }
}
