using Turnwise.Domain.Entities;
using Turnwise.Infrastructure.Persistence;
using Xunit;

namespace Turnwise.Infrastructure.Tests;

public class JsonCharacterTemplateStoreTests
{
    [Fact]
    public async Task SaveThenLoad_RoundTripsCombatantState()
    {
        var store = new JsonCharacterTemplateStore();
        var goblin = new Combatant("Goblin", 7)
        {
            InitiativeFormula = "1d20+2"
        };
        goblin.SetArmorClass(15);
        goblin.AddNamedRoll("Scimitar", Domain.ValueObjects.DiceFormula.Parse("1d6+2"));
        goblin.AddCondition("Poisoned");

        await using var stream = new MemoryStream();
        await store.SaveAsync(goblin, stream);
        stream.Position = 0;
        var loaded = await store.LoadAsync(stream);

        Assert.Equal(goblin.Name, loaded.Name);
        Assert.Equal(goblin.MaxHp, loaded.MaxHp);
        Assert.Equal(goblin.ArmorClass, loaded.ArmorClass);
        Assert.Single(loaded.NamedRolls);
        Assert.Equal("Scimitar", loaded.NamedRolls[0].Name);
        Assert.Single(loaded.Conditions);
        Assert.Equal("Poisoned", loaded.Conditions[0].Name);
    }
}
