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
        var combatant = new Combatant("Goblin", CombatantType.NonPlayerCharacter, 20);
        encounter.AddCombatant(combatant);

        var applied = service.ApplyHpDelta(encounter, combatant.Id, -7);

        Assert.Equal(-7, applied);
        Assert.Equal(13, combatant.CurrentHp);
        var entry = Assert.Single(encounter.Log);
        Assert.Equal(CombatLogEntryType.HpChange, entry.Type);
        Assert.Equal(combatant.Id, entry.CombatantId);
    }

    [Fact]
    public void RollInitiative_SetsInitiativeFromFormulaAndLogs()
    {
        var service = MakeService(15);
        var encounter = new Encounter();
        var combatant = new Combatant("Fighter", CombatantType.PlayerCharacter, 20)
        {
            InitiativeFormula = "1d20+2"
        };
        encounter.AddCombatant(combatant);

        var result = service.RollInitiative(encounter, combatant.Id);

        Assert.Equal(17, result.Total);
        Assert.Equal(17, combatant.Initiative);
        Assert.Contains(encounter.Log, e => e.Type == CombatLogEntryType.InitiativeChange);
    }

    [Fact]
    public void RollInitiative_ThrowsWhenNoFormulaSet()
    {
        var service = MakeService();
        var encounter = new Encounter();
        var combatant = new Combatant("Fighter", CombatantType.PlayerCharacter, 20);
        encounter.AddCombatant(combatant);

        Assert.Throws<InvalidOperationException>(() => service.RollInitiative(encounter, combatant.Id));
    }

    [Fact]
    public void RollNamedRoll_LogsDiceRollEntry()
    {
        var service = MakeService(4, 4);
        var encounter = new Encounter();
        var combatant = new Combatant("Fighter", CombatantType.PlayerCharacter, 20);
        var roll = combatant.AddNamedRoll("Longsword hit", Domain.ValueObjects.DiceFormula.Parse("1d20+4"));
        encounter.AddCombatant(combatant);

        var result = service.RollNamedRoll(encounter, combatant.Id, roll.Id);

        Assert.Equal(8, result.Total); // 4 + 4
        var entry = Assert.Single(encounter.Log);
        Assert.Equal(CombatLogEntryType.DiceRoll, entry.Type);
    }
}
