using Turnwise.Domain.Entities;
using Turnwise.Domain.Enums;
using Xunit;

namespace Turnwise.Domain.Tests;

public class EncounterTests
{
    private static Combatant MakeCombatant(string name, int? initiative = null)
    {
        var combatant = new Combatant(name, CombatantType.NonPlayerCharacter, 10);
        if (initiative is { } value)
        {
            combatant.SetInitiative(value);
        }
        return combatant;
    }

    [Fact]
    public void NextTurn_AdvancesThroughOrderAndWrapsRound()
    {
        var encounter = new Encounter();
        var a = MakeCombatant("A");
        var b = MakeCombatant("B");
        encounter.AddCombatant(a);
        encounter.AddCombatant(b);

        encounter.NextTurn(); // -> A
        Assert.Equal(a.Id, encounter.ActiveCombatantId);
        Assert.Equal(1, encounter.Round);

        encounter.NextTurn(); // -> B
        Assert.Equal(b.Id, encounter.ActiveCombatantId);
        Assert.Equal(1, encounter.Round);

        encounter.NextTurn(); // wraps -> A, round 2
        Assert.Equal(a.Id, encounter.ActiveCombatantId);
        Assert.Equal(2, encounter.Round);
    }

    [Fact]
    public void AutoSortByInitiative_OrdersHighestFirstAndSinksUnset()
    {
        var encounter = new Encounter();
        var low = MakeCombatant("Low", 5);
        var high = MakeCombatant("High", 20);
        var unset = MakeCombatant("Unset");
        encounter.AddCombatant(low);
        encounter.AddCombatant(unset);
        encounter.AddCombatant(high);

        encounter.AutoSortByInitiative();

        Assert.Equal(["High", "Low", "Unset"], encounter.Combatants.Select(c => c.Name));
    }

    [Fact]
    public void Reorder_MovesCombatantToNewPosition()
    {
        var encounter = new Encounter();
        var a = MakeCombatant("A");
        var b = MakeCombatant("B");
        var c = MakeCombatant("C");
        encounter.AddCombatant(a);
        encounter.AddCombatant(b);
        encounter.AddCombatant(c);

        encounter.Reorder(0, 2);

        Assert.Equal(["B", "C", "A"], encounter.Combatants.Select(x => x.Name));
    }

    [Fact]
    public void RemoveCombatant_ReassignsActiveWhenActiveCombatantRemoved()
    {
        var encounter = new Encounter();
        var a = MakeCombatant("A");
        var b = MakeCombatant("B");
        encounter.AddCombatant(a);
        encounter.AddCombatant(b);
        encounter.NextTurn(); // active = A

        encounter.RemoveCombatant(a.Id);

        Assert.Equal(b.Id, encounter.ActiveCombatantId);
    }

    [Fact]
    public void GetLogForCombatant_FiltersSharedLog()
    {
        var encounter = new Encounter();
        var a = MakeCombatant("A");
        var b = MakeCombatant("B");
        encounter.AddLogEntry(new CombatLogEntry(CombatLogEntryType.HpChange, "A takes damage", a.Id));
        encounter.AddLogEntry(new CombatLogEntry(CombatLogEntryType.HpChange, "B takes damage", b.Id));

        var aLog = encounter.GetLogForCombatant(a.Id).ToList();

        Assert.Single(aLog);
        Assert.Equal("A takes damage", aLog[0].Message);
    }
}
