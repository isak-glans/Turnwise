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
        PortraitBase64 = combatant.PortraitBase64,
        MaxHp = combatant.MaxHp,
        CurrentHp = combatant.CurrentHp,
        TemporaryHp = combatant.TemporaryHp,
        ArmorClass = combatant.ArmorClass,
        InitiativeFormula = combatant.InitiativeFormula,
        Initiative = combatant.Initiative,
        Category = combatant.Category?.ToString(),
        Notes = combatant.Notes,
        NamedRolls = combatant.NamedRolls
            .Select(r => new NamedRollDto { Id = r.Id, Name = r.Name, Formula = r.Formula.ToString(), Category = r.Category?.ToString() })
            .ToList(),
        Counters = combatant.Counters
            .Select(c => new CounterDto { Id = c.Id, Name = c.Name, Current = c.Current, Max = c.Max, ShowBar = c.ShowBar, Category = c.Category?.ToString() })
            .ToList(),
        Conditions = combatant.Conditions
            .Select(c => new ConditionDto { Id = c.Id, Name = c.Name })
            .ToList()
    };

    public static Combatant ToDomain(CombatantDto dto)
    {
        CombatantCategory? category = Enum.TryParse<CombatantCategory>(dto.Category, out var parsed) ? parsed : null;

        var combatant = Combatant.Restore(
            dto.Id,
            dto.Name,
            dto.MaxHp,
            dto.CurrentHp,
            dto.PortraitBase64,
            dto.InitiativeFormula,
            dto.Initiative,
            dto.ArmorClass,
            category,
            dto.Notes,
            dto.TemporaryHp);

        foreach (var roll in dto.NamedRolls)
        {
            var rollCategory = Enum.TryParse<NamedRollCategory>(roll.Category, out var parsedRollCategory) ? parsedRollCategory : (NamedRollCategory?)null;
            combatant.AddNamedRoll(roll.Name, DiceFormula.Parse(roll.Formula), roll.Id, rollCategory);
        }

        foreach (var counter in dto.Counters)
        {
            var counterCategory = Enum.TryParse<CounterCategory>(counter.Category, out var parsedCounterCategory) ? parsedCounterCategory : (CounterCategory?)null;
            combatant.AddCounter(counter.Name, counter.Current, counter.Max, counter.ShowBar, counter.Id, counterCategory);
        }

        foreach (var condition in dto.Conditions)
        {
            combatant.AddCondition(condition.Name, condition.Id);
        }

        return combatant;
    }
}
