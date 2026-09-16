using Turnwise.Domain.Entities;
using Turnwise.Domain.Enums;
using Turnwise.Infrastructure.Persistence;
using Xunit;

namespace Turnwise.Infrastructure.Tests;

public class JsonCharacterTemplateStoreTests
{
    [Fact]
    public async Task SaveThenLoad_RoundTripsCombatantState()
    {
        var store = new JsonCharacterTemplateStore();
        var goblin = new Combatant("Goblin", CombatantType.NonPlayerCharacter, 7)
        {
            Description = "A sneaky goblin.",
            InitiativeFormula = "1d20+2"
        };
        goblin.AddNamedRoll("Scimitar", Domain.ValueObjects.DiceFormula.Parse("1d6+2"));

        await using var stream = new MemoryStream();
        await store.SaveAsync(goblin, stream);
        stream.Position = 0;
        var loaded = await store.LoadAsync(stream);

        Assert.Equal(goblin.Name, loaded.Name);
        Assert.Equal(goblin.Type, loaded.Type);
        Assert.Equal(goblin.MaxHp, loaded.MaxHp);
        Assert.Equal(goblin.Description, loaded.Description);
        Assert.Single(loaded.NamedRolls);
        Assert.Equal("Scimitar", loaded.NamedRolls[0].Name);
    }
}
