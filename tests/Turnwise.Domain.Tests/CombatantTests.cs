using Turnwise.Domain.Entities;
using Xunit;

namespace Turnwise.Domain.Tests;

public class CombatantTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_RejectsEmptyOrWhitespaceName(string? name)
    {
        Assert.Throws<ArgumentException>(() => new Combatant(name!, 10));
    }

    [Fact]
    public void Constructor_RejectsNegativeMaxHp()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Combatant("Goblin", -1));
    }

    [Fact]
    public void SetMaxHp_RejectsNegativeValue()
    {
        var combatant = new Combatant("Goblin", 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => combatant.SetMaxHp(-1));
    }

    [Fact]
    public void ApplyHpDelta_ExtremePositiveDeltaDoesNotOverflowPastMaxHp()
    {
        var combatant = new Combatant("Goblin", 10);
        combatant.ApplyHpDelta(-7); // leave room to heal, so the clamp is actually exercised

        var applied = combatant.ApplyHpDelta(int.MaxValue);

        Assert.Equal(10, combatant.CurrentHp);
        Assert.Equal(7, applied);
    }

    [Fact]
    public void ApplyHpDelta_ExtremeNegativeDeltaDoesNotOverflowPastZero()
    {
        var combatant = new Combatant("Goblin", 10);

        var applied = combatant.ApplyHpDelta(int.MinValue);

        Assert.Equal(0, combatant.CurrentHp);
        Assert.Equal(-10, applied);
    }

    [Fact]
    public void ApplyHpDelta_ClampsAtZero()
    {
        var combatant = new Combatant("Goblin", 10);

        var applied = combatant.ApplyHpDelta(-25);

        Assert.Equal(0, combatant.CurrentHp);
        Assert.Equal(-10, applied);
    }

    [Fact]
    public void ApplyHpDelta_ClampsAtMaxHp()
    {
        var combatant = new Combatant("Goblin", 10);
        combatant.ApplyHpDelta(-5);

        var applied = combatant.ApplyHpDelta(100);

        Assert.Equal(10, combatant.CurrentHp);
        Assert.Equal(5, applied);
    }

    [Fact]
    public void IsDefeated_TrueAtZeroHp()
    {
        var combatant = new Combatant("Goblin", 10);
        combatant.ApplyHpDelta(-10);

        Assert.True(combatant.IsDefeated);
    }

    [Fact]
    public void ClearInitiative_UnsetsInitiative()
    {
        var combatant = new Combatant("Goblin", 10);
        combatant.SetInitiative(15);

        combatant.ClearInitiative();

        Assert.Null(combatant.Initiative);
    }

    [Fact]
    public void Duplicate_CopiesStateWithNewIdAndClearedInitiative()
    {
        var original = new Combatant("Goblin", 10);
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
    public void Name_LongerThanMaxIsSilentlyTruncated()
    {
        var combatant = new Combatant("Goblin", 10);

        combatant.Name = new string('x', Combatant.MaxNameLength + 50);

        Assert.Equal(Combatant.MaxNameLength, combatant.Name.Length);
    }

    [Fact]
    public void Constructor_NameLongerThanMaxIsSilentlyTruncated()
    {
        var combatant = new Combatant(new string('x', Combatant.MaxNameLength + 50), 10);

        Assert.Equal(Combatant.MaxNameLength, combatant.Name.Length);
    }

    [Fact]
    public void SetArmorClass_DefaultsToNullAndCanBeClearedAgain()
    {
        var combatant = new Combatant("Goblin", 10);
        Assert.Null(combatant.ArmorClass);

        combatant.SetArmorClass(15);
        Assert.Equal(15, combatant.ArmorClass);

        combatant.SetArmorClass(null);
        Assert.Null(combatant.ArmorClass);
    }

    [Fact]
    public void SetArmorClass_RejectsNegativeValue()
    {
        var combatant = new Combatant("Goblin", 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => combatant.SetArmorClass(-1));
    }

    [Fact]
    public void Duplicate_CopiesArmorClass()
    {
        var original = new Combatant("Goblin", 10);
        original.SetArmorClass(13);

        var clone = original.Duplicate();

        Assert.Equal(13, clone.ArmorClass);
    }

    [Fact]
    public void Category_DefaultsToNullAndIsSettable()
    {
        var combatant = new Combatant("Goblin", 10);
        Assert.Null(combatant.Category);

        combatant.Category = Turnwise.Domain.Enums.CombatantCategory.Enemy;

        Assert.Equal(Turnwise.Domain.Enums.CombatantCategory.Enemy, combatant.Category);
    }

    [Fact]
    public void Notes_LongerThanMaxIsSilentlyTruncated()
    {
        var combatant = new Combatant("Goblin", 10);

        combatant.Notes = new string('x', Combatant.MaxNotesLength + 50);

        Assert.Equal(Combatant.MaxNotesLength, combatant.Notes.Length);
    }

    [Fact]
    public void Duplicate_CopiesCategoryAndNotes()
    {
        var original = new Combatant("Goblin", 10)
        {
            Category = Turnwise.Domain.Enums.CombatantCategory.Ally,
            Notes = "Secretly a good goblin."
        };

        var clone = original.Duplicate();

        Assert.Equal(Turnwise.Domain.Enums.CombatantCategory.Ally, clone.Category);
        Assert.Equal("Secretly a good goblin.", clone.Notes);
    }

    [Fact]
    public void Clone_PreservesIdAndEverySubEntityId_UnlikeDuplicate()
    {
        var original = new Combatant("Goblin", 10);
        original.SetInitiative(14);
        original.ApplyHpDelta(-3);
        var roll = original.AddNamedRoll("Scimitar", Turnwise.Domain.ValueObjects.DiceFormula.Parse("1d6+2"));
        var counter = original.AddCounter("Rage", 1, 2);
        var condition = original.AddCondition("Poisoned");

        var clone = original.Clone();

        Assert.Equal(original.Id, clone.Id);
        Assert.Equal(original.CurrentHp, clone.CurrentHp);
        Assert.Equal(original.Initiative, clone.Initiative);
        Assert.Equal(roll.Id, Assert.Single(clone.NamedRolls).Id);
        Assert.Equal(counter.Id, Assert.Single(clone.Counters).Id);
        Assert.Equal(condition.Id, Assert.Single(clone.Conditions).Id);
    }

    [Fact]
    public void Duplicate_And_Clone_CopyNamedRollCategory()
    {
        var original = new Combatant("Goblin", 10);
        original.AddNamedRoll("Scimitar", Turnwise.Domain.ValueObjects.DiceFormula.Parse("1d6+2"), category: Turnwise.Domain.Enums.NamedRollCategory.Weapon);

        var duplicate = original.Duplicate();
        var clone = original.Clone();

        Assert.Equal(Turnwise.Domain.Enums.NamedRollCategory.Weapon, duplicate.NamedRolls[0].Category);
        Assert.Equal(Turnwise.Domain.Enums.NamedRollCategory.Weapon, clone.NamedRolls[0].Category);
    }

    [Fact]
    public void Clone_IsIndependentOfTheOriginal()
    {
        var original = new Combatant("Goblin", 10);
        var counter = original.AddCounter("Rage", 1, 2);

        var clone = original.Clone();
        original.ApplyHpDelta(-5);
        counter.Adjust(1);

        Assert.Equal(10, clone.CurrentHp);
        Assert.Equal(1, clone.Counters[0].Current);
    }

    [Fact]
    public void AddCounter_RejectsNegativeMax()
    {
        var combatant = new Combatant("Fighter", 20);

        Assert.Throws<ArgumentOutOfRangeException>(() => combatant.AddCounter("Ki", -1, -1));
    }

    [Fact]
    public void AddNamedRoll_RejectsEmptyName()
    {
        var combatant = new Combatant("Fighter", 20);
        var formula = Turnwise.Domain.ValueObjects.DiceFormula.Parse("1d6");

        Assert.Throws<ArgumentException>(() => combatant.AddNamedRoll("", formula));
    }

    [Fact]
    public void AddCondition_ThenRemoveCondition_RoundTrips()
    {
        var combatant = new Combatant("Fighter", 20);

        var condition = combatant.AddCondition("Prone");
        Assert.Single(combatant.Conditions);

        combatant.RemoveCondition(condition.Id);
        Assert.Empty(combatant.Conditions);
    }

    [Fact]
    public void ReorderNamedRoll_MovesRollToNewPosition()
    {
        var combatant = new Combatant("Fighter", 20);
        var formula = Turnwise.Domain.ValueObjects.DiceFormula.Parse("1d6");
        combatant.AddNamedRoll("A", formula);
        combatant.AddNamedRoll("B", formula);
        combatant.AddNamedRoll("C", formula);

        combatant.ReorderNamedRoll(0, 2);

        Assert.Equal(["B", "C", "A"], combatant.NamedRolls.Select(r => r.Name));
    }

    [Fact]
    public void ReorderCounter_MovesCounterToNewPosition()
    {
        var combatant = new Combatant("Fighter", 20);
        combatant.AddCounter("A", 1, 1);
        combatant.AddCounter("B", 1, 1);
        combatant.AddCounter("C", 1, 1);

        combatant.ReorderCounter(2, 0);

        Assert.Equal(["C", "A", "B"], combatant.Counters.Select(c => c.Name));
    }

    [Fact]
    public void DuplicateNamedRoll_InsertsCopyRightAfterOriginal()
    {
        var combatant = new Combatant("Fighter", 20);
        combatant.AddNamedRoll("Longsword", Turnwise.Domain.ValueObjects.DiceFormula.Parse("1d20+4"));
        combatant.AddNamedRoll("Dagger", Turnwise.Domain.ValueObjects.DiceFormula.Parse("1d20+2"));

        var clone = combatant.DuplicateNamedRoll(combatant.NamedRolls[0].Id);

        Assert.Equal(["Longsword", "Longsword", "Dagger"], combatant.NamedRolls.Select(r => r.Name));
        Assert.NotEqual(combatant.NamedRolls[0].Id, clone.Id);
        Assert.Equal(combatant.NamedRolls[0].Formula, clone.Formula);
    }

    [Fact]
    public void DuplicateCounter_InsertsCopyRightAfterOriginal()
    {
        var combatant = new Combatant("Fighter", 20);
        combatant.AddCounter("Ki", 3, 4);
        combatant.AddCounter("Ammo", 10, 20);

        var clone = combatant.DuplicateCounter(combatant.Counters[0].Id);

        Assert.Equal(["Ki", "Ki", "Ammo"], combatant.Counters.Select(c => c.Name));
        Assert.NotEqual(combatant.Counters[0].Id, clone.Id);
        Assert.Equal(3, clone.Current);
        Assert.Equal(4, clone.Max);
    }
}
