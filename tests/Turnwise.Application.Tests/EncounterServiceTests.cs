using Turnwise.Application.Dice;
using Turnwise.Application.Services;
using Turnwise.Domain.Entities;
using Turnwise.Domain.Enums;
using Xunit;

namespace Turnwise.Application.Tests;

public class EncounterServiceTests
{
    private static EncounterService MakeService(params int[] rolls) =>
        new(new DiceRollingService(new FakeRandomSource(rolls)));

    [Fact]
    public void ApplyHpDelta_UpdatesHpAndLogsEntry()
    {
        var service = MakeService();
        var encounter = new Encounter();
        var combatant = new Combatant("Goblin", 20);
        encounter.AddCombatant(combatant);

        var applied = service.ApplyHpDelta(encounter, combatant.Id, -7);

        Assert.Equal(-7, applied);
        Assert.Equal(13, combatant.CurrentHp);
        var entry = Assert.Single(encounter.Log);
        Assert.Equal(CombatLogEntryType.HpChange, entry.Type);
        Assert.Equal(combatant.Id, entry.CombatantId);
        Assert.Equal("-7", entry.Result);
    }

    [Fact]
    public void RollInitiative_SetsInitiativeFromFormulaAndLogs()
    {
        var service = MakeService(15);
        var encounter = new Encounter();
        var combatant = new Combatant("Fighter", 20)
        {
            InitiativeFormula = "1d20+2"
        };
        encounter.AddCombatant(combatant);

        var result = service.RollInitiative(encounter, combatant.Id);

        Assert.Equal(17, result.Total);
        Assert.Equal(17, combatant.Initiative);
        Assert.Contains(encounter.Log, e => e.Type == CombatLogEntryType.InitiativeChange);
        Assert.Equal("17", Assert.Single(encounter.Log).Result);
    }

    [Fact]
    public void RollInitiative_ThrowsWhenNoFormulaSet()
    {
        var service = MakeService();
        var encounter = new Encounter();
        var combatant = new Combatant("Fighter", 20);
        encounter.AddCombatant(combatant);

        Assert.Throws<InvalidOperationException>(() => service.RollInitiative(encounter, combatant.Id));
    }

    [Fact]
    public void DuplicateCombatant_InsertsCloneRightAfterOriginal()
    {
        var service = MakeService();
        var encounter = new Encounter();
        var goblin = new Combatant("Goblin", 7);
        var fighter = new Combatant("Fighter", 20);
        encounter.AddCombatant(goblin);
        encounter.AddCombatant(fighter);

        var clone = service.DuplicateCombatant(encounter, goblin.Id);

        Assert.Equal(["Goblin", "Goblin", "Fighter"], encounter.Combatants.Select(c => c.Name));
        Assert.Equal(clone.Id, encounter.Combatants[1].Id);
        Assert.NotEqual(goblin.Id, clone.Id);
    }

    [Fact]
    public void ApplyHpDeltaToMany_AppliesSameDeltaToEveryCombatant()
    {
        var service = MakeService();
        var encounter = new Encounter();
        var a = new Combatant("A", 20);
        var b = new Combatant("B", 10);
        encounter.AddCombatant(a);
        encounter.AddCombatant(b);

        service.ApplyHpDeltaToMany(encounter, [a.Id, b.Id], -6);

        Assert.Equal(14, a.CurrentHp);
        Assert.Equal(4, b.CurrentHp);
        Assert.Equal(2, encounter.Log.Count);
    }

    [Fact]
    public void ApplyRolledDamageToMany_WithSharedRoll_AppliesSameTotalToEveryCombatant()
    {
        var service = MakeService(5); // 1d6 -> 5, reused for both since only one roll should be consumed
        var encounter = new Encounter();
        var a = new Combatant("A", 20);
        var b = new Combatant("B", 20);
        encounter.AddCombatant(a);
        encounter.AddCombatant(b);
        var formula = Domain.ValueObjects.DiceFormula.Parse("1d6");

        service.ApplyRolledDamageToMany(encounter, [a.Id, b.Id], formula, sharedRoll: true);

        Assert.Equal(15, a.CurrentHp);
        Assert.Equal(15, b.CurrentHp);
        Assert.All(encounter.Log, e => Assert.Equal("-5", e.Result));
    }

    [Fact]
    public void ApplyRolledDamageToMany_WithIndividualRolls_RollsSeparatelyPerCombatant()
    {
        var service = MakeService(2, 6); // first combatant rolls 2, second rolls 6
        var encounter = new Encounter();
        var a = new Combatant("A", 20);
        var b = new Combatant("B", 20);
        encounter.AddCombatant(a);
        encounter.AddCombatant(b);
        var formula = Domain.ValueObjects.DiceFormula.Parse("1d6");

        service.ApplyRolledDamageToMany(encounter, [a.Id, b.Id], formula, sharedRoll: false);

        Assert.Equal(18, a.CurrentHp);
        Assert.Equal(14, b.CurrentHp);
    }

    [Fact]
    public void AddConditionToMany_AddsConditionToEveryCombatant()
    {
        var service = MakeService();
        var encounter = new Encounter();
        var a = new Combatant("A", 20);
        var b = new Combatant("B", 20);
        encounter.AddCombatant(a);
        encounter.AddCombatant(b);

        service.AddConditionToMany(encounter, [a.Id, b.Id], "Prone");

        Assert.Equal("Prone", Assert.Single(a.Conditions).Name);
        Assert.Equal("Prone", Assert.Single(b.Conditions).Name);
    }

    [Fact]
    public void RollInitiativeForMany_SkipsCombatantsWithoutAFormula()
    {
        var service = MakeService(10);
        var encounter = new Encounter();
        var withFormula = new Combatant("A", 20) { InitiativeFormula = "1d20" };
        var withoutFormula = new Combatant("B", 20);
        encounter.AddCombatant(withFormula);
        encounter.AddCombatant(withoutFormula);

        var results = service.RollInitiativeForMany(encounter, [withFormula.Id, withoutFormula.Id]);

        Assert.Single(results);
        Assert.True(results.ContainsKey(withFormula.Id));
        Assert.Equal(10, withFormula.Initiative);
        Assert.Null(withoutFormula.Initiative);
    }

    [Fact]
    public void RollNamedRoll_LogsDiceRollEntry()
    {
        var service = MakeService(4, 4);
        var encounter = new Encounter();
        var combatant = new Combatant("Fighter", 20);
        var roll = combatant.AddNamedRoll("Longsword hit", Domain.ValueObjects.DiceFormula.Parse("1d20+4"));
        encounter.AddCombatant(combatant);

        var result = service.RollNamedRoll(encounter, combatant.Id, roll.Id);

        Assert.Equal(8, result.Total); // 4 + 4
        var entry = Assert.Single(encounter.Log);
        Assert.Equal(CombatLogEntryType.DiceRoll, entry.Type);
        Assert.Equal("8", entry.Result);
    }
}
