using Turnwise.Domain.Entities;
using Turnwise.Domain.Enums;
using Xunit;

namespace Turnwise.Domain.Tests;

public class CombatantTests
{
    [Fact]
    public void ApplyHpDelta_ClampsAtZero()
    {
        var combatant = new Combatant("Goblin", CombatantType.NonPlayerCharacter, 10);

        var applied = combatant.ApplyHpDelta(-25);

        Assert.Equal(0, combatant.CurrentHp);
        Assert.Equal(-10, applied);
    }

    [Fact]
    public void ApplyHpDelta_ClampsAtMaxHp()
    {
        var combatant = new Combatant("Goblin", CombatantType.NonPlayerCharacter, 10);
        combatant.ApplyHpDelta(-5);

        var applied = combatant.ApplyHpDelta(100);

        Assert.Equal(10, combatant.CurrentHp);
        Assert.Equal(5, applied);
    }

    [Fact]
    public void IsDefeated_TrueAtZeroHp()
    {
        var combatant = new Combatant("Goblin", CombatantType.NonPlayerCharacter, 10);
        combatant.ApplyHpDelta(-10);

        Assert.True(combatant.IsDefeated);
    }

    [Fact]
    public void ClearInitiative_UnsetsInitiative()
    {
        var combatant = new Combatant("Goblin", CombatantType.NonPlayerCharacter, 10);
        combatant.SetInitiative(15);

        combatant.ClearInitiative();

        Assert.Null(combatant.Initiative);
    }

    [Fact]
    public void Duplicate_CopiesStateWithNewIdAndClearedInitiative()
    {
        var original = new Combatant("Goblin", CombatantType.NonPlayerCharacter, 10);
        original.SetInitiative(14);
        original.ApplyHpDelta(-3);
        original.AddCounter("Rage", 1, 2);
        original.AddNamedRoll("Scimitar", Turnwise.Domain.ValueObjects.DiceFormula.Parse("1d6+2"));
        original.AddCondition("Poisoned");

        var clone = original.Duplicate();

        Assert.NotEqual(original.Id, clone.Id);
        Assert.Equal(original.Name, clone.Name);
        Assert.Equal(original.CurrentHp, clone.CurrentHp);
        Assert.Equal(original.MaxHp, clone.MaxHp);
        Assert.Null(clone.Initiative);
        Assert.Single(clone.Counters);
        Assert.Single(clone.NamedRolls);
        Assert.Single(clone.Conditions);
        Assert.Equal("Poisoned", clone.Conditions[0].Name);
    }

    [Fact]
    public void AddCondition_ThenRemoveCondition_RoundTrips()
    {
        var combatant = new Combatant("Fighter", CombatantType.PlayerCharacter, 20);

        var condition = combatant.AddCondition("Prone");
        Assert.Single(combatant.Conditions);

        combatant.RemoveCondition(condition.Id);
        Assert.Empty(combatant.Conditions);
    }
}
