using Turnwise.Application.Common;
using Turnwise.Domain.Entities;
using Turnwise.Domain.Enums;
using Turnwise.Infrastructure.Persistence;
using Xunit;

namespace Turnwise.Infrastructure.Tests;

public class JsonEncounterStoreTests
{
    [Fact]
    public async Task SaveThenLoad_RoundTripsEncounterState()
    {
        var store = new JsonEncounterStore();
        var encounter = new Encounter("Goblin Ambush");
        var fighter = new Combatant("Fighter", 30) { InitiativeFormula = "1d20+2" };
        fighter.SetInitiative(18);
        fighter.AddCounter("Superiority Dice", 3, 4);
        fighter.AddNamedRoll("Longsword hit", Domain.ValueObjects.DiceFormula.Parse("1d20+4"));
        encounter.AddCombatant(fighter);
        encounter.NextTurn();
        encounter.AddLogEntry(new CombatLogEntry(CombatLogEntryType.InitiativeChange, "Fighter: initiative 18", fighter.Id));

        await using var stream = new MemoryStream();
        await store.SaveAsync(encounter, stream);
        stream.Position = 0;
        var loaded = await store.LoadAsync(stream);

        Assert.Equal(encounter.Id, loaded.Id);
        Assert.Equal(encounter.Name, loaded.Name);
        Assert.Equal(encounter.Round, loaded.Round);
        Assert.Equal(encounter.ActiveCombatantId, loaded.ActiveCombatantId);
        var loadedFighter = Assert.Single(loaded.Combatants);
        Assert.Equal(fighter.Name, loadedFighter.Name);
        Assert.Equal(fighter.Initiative, loadedFighter.Initiative);
        Assert.Equal(3, loadedFighter.Counters[0].Current);
        Assert.Single(loadedFighter.NamedRolls);
        Assert.Single(loaded.Log);
    }

    [Fact]
    public async Task SaveThenLoad_DeduplicatesIdenticalPortraitsAcrossCombatants()
    {
        var store = new JsonEncounterStore();
        var encounter = new Encounter("Goblin Ambush");
        const string sharedPortrait = "aGVsbG8="; // "hello" - stand-in for real image bytes
        var goblinA = new Combatant("Goblin A", 7) { PortraitBase64 = sharedPortrait };
        var goblinB = new Combatant("Goblin B", 7) { PortraitBase64 = sharedPortrait };
        encounter.AddCombatant(goblinA);
        encounter.AddCombatant(goblinB);

        await using var stream = new MemoryStream();
        await store.SaveAsync(encounter, stream);
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        // The shared portrait should appear exactly once in the saved file, not once per combatant.
        var occurrences = System.Text.RegularExpressions.Regex.Matches(json, System.Text.RegularExpressions.Regex.Escape(sharedPortrait)).Count;
        Assert.Equal(1, occurrences);

        stream.Position = 0;
        var loaded = await store.LoadAsync(stream);

        Assert.All(loaded.Combatants, c => Assert.Equal(sharedPortrait, c.PortraitBase64));
    }

    [Fact]
    public async Task LoadAsync_RejectsFileOlderThanMinSupportedVersion()
    {
        var store = new JsonEncounterStore();
        var json = """{"SchemaVersion":0,"Id":"11111111-1111-1111-1111-111111111111","Name":"Old","Round":1}""";
        await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        await Assert.ThrowsAsync<UnsupportedSchemaVersionException>(() => store.LoadAsync(stream));
    }
}
