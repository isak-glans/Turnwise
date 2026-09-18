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
        var goblin = new Combatant("Goblin", 7)
        {
            InitiativeFormula = "1d20+2",
            Category = CombatantCategory.Enemy,
            Notes = "Hiding in the bushes."
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
        Assert.Equal(CombatantCategory.Enemy, loaded.Category);
        Assert.Equal("Hiding in the bushes.", loaded.Notes);
        Assert.Single(loaded.NamedRolls);
        Assert.Equal("Scimitar", loaded.NamedRolls[0].Name);
        Assert.Single(loaded.Conditions);
        Assert.Equal("Poisoned", loaded.Conditions[0].Name);
    }

    [Fact]
    public async Task Load_FileWithoutCategoryOrNotesFields_DefaultsToNullAndEmpty()
    {
        // Simulates a character file saved before Category/Notes existed (schema version 3).
        var json = """
        {
          "SchemaVersion": 3,
          "Character": {
            "Id": "11111111-1111-1111-1111-111111111111",
            "Name": "Old Goblin",
            "MaxHp": 7,
            "MaxHpLocked": true,
            "CurrentHp": 7,
            "InitiativeLocked": true
          }
        }
        """;
        var store = new JsonCharacterTemplateStore();
        await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        var loaded = await store.LoadAsync(stream);

        Assert.Null(loaded.Category);
        Assert.Equal("", loaded.Notes);
    }
}
