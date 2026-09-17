using Turnwise.Application.Dice;
using Turnwise.Domain.Entities;
using Turnwise.Domain.Enums;
using Turnwise.Domain.ValueObjects;

namespace Turnwise.Application.Services;

/// <summary>
/// Orchestrates the core combat-tracking use cases against an in-memory <see cref="Encounter"/>:
/// turn order, HP changes, initiative rolls and named rolls - recording each to the combat log
/// where the spec calls for it (dice rolls, HP changes, initiative rolls/changes).
/// </summary>
public sealed class EncounterService(DiceRollingService diceRoller)
{
    public void AddCombatant(Encounter encounter, Combatant combatant, int? atIndex = null) =>
        encounter.AddCombatant(combatant, atIndex);

    public void RemoveCombatant(Encounter encounter, Guid combatantId) =>
        encounter.RemoveCombatant(combatantId);

    /// <summary>Adds an independent copy of a combatant right after the original in turn order.</summary>
    public Combatant DuplicateCombatant(Encounter encounter, Guid combatantId)
    {
        var combatants = encounter.Combatants;
        var index = -1;
        Combatant? original = null;
        for (var i = 0; i < combatants.Count; i++)
        {
            if (combatants[i].Id == combatantId)
            {
                index = i;
                original = combatants[i];
                break;
            }
        }

        if (original is null)
        {
            throw new KeyNotFoundException($"Combatant {combatantId} is not part of this encounter.");
        }

        var clone = original.Duplicate();
        encounter.AddCombatant(clone, index + 1);
        return clone;
    }

    public void Reorder(Encounter encounter, int fromIndex, int toIndex) =>
        encounter.Reorder(fromIndex, toIndex);

    public void AutoSort(Encounter encounter) => encounter.AutoSortByInitiative();

    public void NextTurn(Encounter encounter) => encounter.NextTurn();

    public int ApplyHpDelta(Encounter encounter, Guid combatantId, int delta)
    {
        var combatant = GetCombatant(encounter, combatantId);
        var applied = combatant.ApplyHpDelta(delta);

        var direction = applied < 0 ? "damage" : "healing";
        var message = $"{combatant.Name}: {(applied >= 0 ? "+" : "")}{applied} HP ({direction}) -> {combatant.CurrentHp}/{combatant.MaxHp}";
        encounter.AddLogEntry(new CombatLogEntry(CombatLogEntryType.HpChange, message, combatant.Id));

        return applied;
    }

    /// <summary>Applies the same flat HP delta (negative = damage, positive = healing) to every given combatant.</summary>
    public IReadOnlyDictionary<Guid, int> ApplyHpDeltaToMany(Encounter encounter, IEnumerable<Guid> combatantIds, int delta)
    {
        var results = new Dictionary<Guid, int>();
        foreach (var id in combatantIds.ToList())
        {
            results[id] = ApplyHpDelta(encounter, id, delta);
        }

        return results;
    }

    /// <summary>
    /// Rolls damage and applies it to every given combatant. With <paramref name="sharedRoll"/> the
    /// dice are rolled once and every combatant takes that same amount; otherwise each combatant
    /// takes an independent roll of the same formula.
    /// </summary>
    public IReadOnlyDictionary<Guid, DiceRollResult> ApplyRolledDamageToMany(
        Encounter encounter, IEnumerable<Guid> combatantIds, DiceFormula formula, bool sharedRoll)
    {
        var results = new Dictionary<Guid, DiceRollResult>();
        var shared = sharedRoll ? diceRoller.Roll(formula) : null;

        foreach (var id in combatantIds.ToList())
        {
            var combatant = GetCombatant(encounter, id);
            var roll = shared ?? diceRoller.Roll(formula);
            results[id] = roll;

            combatant.ApplyHpDelta(-roll.Total);
            var message = $"{combatant.Name}: {roll} damage -> {combatant.CurrentHp}/{combatant.MaxHp}";
            encounter.AddLogEntry(new CombatLogEntry(CombatLogEntryType.HpChange, message, combatant.Id));
        }

        return results;
    }

    /// <summary>Adds the same condition to every given combatant.</summary>
    public void AddConditionToMany(Encounter encounter, IEnumerable<Guid> combatantIds, string conditionName)
    {
        foreach (var id in combatantIds.ToList())
        {
            GetCombatant(encounter, id).AddCondition(conditionName);
        }
    }

    public DiceRollResult RollInitiative(Encounter encounter, Guid combatantId)
    {
        var combatant = GetCombatant(encounter, combatantId);
        var result = RollFormula(combatant.InitiativeFormula, combatant.Name);

        combatant.SetInitiative(result.Total);
        encounter.AddLogEntry(new CombatLogEntry(
            CombatLogEntryType.InitiativeChange,
            $"{combatant.Name}: initiative {result} = {result.Total}",
            combatant.Id));

        return result;
    }

    /// <summary>Bulk-rolls initiative for the given combatants (each gets its own independent roll). Combatants with no formula set are skipped.</summary>
    public IReadOnlyDictionary<Guid, DiceRollResult> RollInitiativeForMany(Encounter encounter, IEnumerable<Guid> combatantIds)
    {
        var results = new Dictionary<Guid, DiceRollResult>();
        foreach (var id in combatantIds.ToList())
        {
            var combatant = GetCombatant(encounter, id);
            if (string.IsNullOrWhiteSpace(combatant.InitiativeFormula))
            {
                continue;
            }

            results[id] = RollInitiative(encounter, id);
        }

        return results;
    }

    public DiceRollResult RollNamedRoll(Encounter encounter, Guid combatantId, Guid rollId)
    {
        var combatant = GetCombatant(encounter, combatantId);
        var namedRoll = combatant.NamedRolls.FirstOrDefault(r => r.Id == rollId)
            ?? throw new KeyNotFoundException($"Combatant {combatantId} has no roll {rollId}.");

        var result = diceRoller.Roll(namedRoll.Formula);
        encounter.AddLogEntry(new CombatLogEntry(
            CombatLogEntryType.DiceRoll,
            $"{combatant.Name}: {namedRoll.Name} ({result}) = {result.Total}",
            combatant.Id));

        return result;
    }

    private DiceRollResult RollFormula(string? formulaText, string combatantName)
    {
        if (!DiceFormula.TryParse(formulaText, out var formula))
        {
            throw new InvalidOperationException($"{combatantName} has no valid initiative formula set.");
        }

        return diceRoller.Roll(formula!);
    }

    private static Combatant GetCombatant(Encounter encounter, Guid combatantId) =>
        encounter.Combatants.FirstOrDefault(c => c.Id == combatantId)
            ?? throw new KeyNotFoundException($"Combatant {combatantId} is not part of this encounter.");
}
