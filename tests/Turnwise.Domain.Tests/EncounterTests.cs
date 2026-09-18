using Turnwise.Domain.Entities;
using Turnwise.Domain.Enums;
using Xunit;

namespace Turnwise.Domain.Tests;

public class EncounterTests
{
    private static Combatant MakeCombatant(string name, int? initiative = null)
    {
        var combatant = new Combatant(name, 10);
        if (initiative is { } value)
        {
            combatant.SetInitiative(value);
        }
        return combatant;
    }

    [Fact]
    public void Name_LongerThanMaxIsSilentlyTruncated()
    {
        var encounter = new Encounter();

        encounter.Name = new string('x', Encounter.MaxNameLength + 50);

        Assert.Equal(Encounter.MaxNameLength, encounter.Name.Length);
    }

    [Fact]
    public void Constructor_NameLongerThanMaxIsSilentlyTruncated()
    {
        var encounter = new Encounter(new string('x', Encounter.MaxNameLength + 50));

        Assert.Equal(Encounter.MaxNameLength, encounter.Name.Length);
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
    public void PreviousTurn_ReversesThroughOrderAndWrapsRoundDown()
    {
        var encounter = new Encounter();
        var a = MakeCombatant("A");
        var b = MakeCombatant("B");
        encounter.AddCombatant(a);
        encounter.AddCombatant(b);
        encounter.NextTurn(); // -> A, round 1
        encounter.NextTurn(); // -> B, round 1
        encounter.NextTurn(); // wraps -> A, round 2

        encounter.PreviousTurn(); // -> B, back to round 1
        Assert.Equal(b.Id, encounter.ActiveCombatantId);
        Assert.Equal(1, encounter.Round);

        encounter.PreviousTurn(); // -> A, round 1
        Assert.Equal(a.Id, encounter.ActiveCombatantId);
        Assert.Equal(1, encounter.Round);
    }

    [Fact]
    public void PreviousTurn_AtStartOfRound1_WrapsToLastCombatantWithoutGoingBelowRound1()
    {
        var encounter = new Encounter();
        var a = MakeCombatant("A");
        var b = MakeCombatant("B");
        encounter.AddCombatant(a);
        encounter.AddCombatant(b);
        encounter.NextTurn(); // -> A, round 1

        encounter.PreviousTurn(); // wraps -> B, round stays 1

        Assert.Equal(b.Id, encounter.ActiveCombatantId);
        Assert.Equal(1, encounter.Round);
    }

    [Fact]
    public void PreviousTurn_WithNoActiveTurn_IsNoOp()
    {
        var encounter = new Encounter();
        var a = MakeCombatant("A");
        encounter.AddCombatant(a);

        encounter.PreviousTurn();

        Assert.Null(encounter.ActiveCombatantId);
        Assert.Equal(1, encounter.Round);
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
    public void NextTurn_LogsRoundChangeOnlyWhenRoundWraps()
    {
        var encounter = new Encounter();
        var a = MakeCombatant("A");
        var b = MakeCombatant("B");
        encounter.AddCombatant(a);
        encounter.AddCombatant(b);

        encounter.NextTurn(); // -> A, round 1 (no wrap yet)
        encounter.NextTurn(); // -> B, round 1 (no wrap yet)
        Assert.Empty(encounter.Log);

        encounter.NextTurn(); // wraps -> A, round 2
        Assert.Single(encounter.Log);
        Assert.Equal(CombatLogEntryType.RoundChange, encounter.Log[0].Type);
        Assert.Equal("Round 2", encounter.Log[0].Message);
    }
}
