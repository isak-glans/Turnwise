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
}
