using Turnwise.Application.Abstractions;
using Turnwise.Application.Services;
using Turnwise.Domain.Entities;

namespace Turnwise.Application.Tests;

public class CharacterTemplateServiceTests
{
    private sealed class FakeCharacterTemplateStore(Combatant toReturn) : ICharacterTemplateStore
    {
        public Task<Combatant> LoadAsync(Stream stream, CancellationToken cancellationToken = default) =>
            Task.FromResult(toReturn);

        public Task SaveAsync(Combatant combatant, Stream stream, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    [Fact]
    public async Task LoadFromTemplateAsync_MintsAFreshIdDistinctFromTheStoredCombatant()
    {
        var stored = new Combatant("Goblin", 7);
        stored.SetInitiative(12);
        var service = new CharacterTemplateService(new FakeCharacterTemplateStore(stored));

        var loaded = await service.LoadFromTemplateAsync(Stream.Null);

        Assert.NotEqual(stored.Id, loaded.Id);
        Assert.Null(loaded.Initiative);
        Assert.Equal(stored.Name, loaded.Name);
    }

    [Fact]
    public async Task LoadFromTemplateAsync_CalledTwice_YieldsTwoDistinctIds()
    {
        var stored = new Combatant("Goblin", 7);
        var service = new CharacterTemplateService(new FakeCharacterTemplateStore(stored));

        var first = await service.LoadFromTemplateAsync(Stream.Null);
        var second = await service.LoadFromTemplateAsync(Stream.Null);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public async Task LoadFromTemplateAsync_CanBeAddedToAnEncounterAlreadyContainingTheStoredCombatant()
    {
        var stored = new Combatant("Goblin", 7);
        var encounter = new Encounter();
        encounter.AddCombatant(stored); // simulates the combatant the template was saved from still being in the encounter
        var service = new CharacterTemplateService(new FakeCharacterTemplateStore(stored));

        var loaded = await service.LoadFromTemplateAsync(Stream.Null);

        var exception = Record.Exception(() => encounter.AddCombatant(loaded));
        Assert.Null(exception);
    }
}
