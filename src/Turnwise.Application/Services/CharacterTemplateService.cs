using Turnwise.Application.Abstractions;
using Turnwise.Domain.Entities;

namespace Turnwise.Application.Services;

/// <summary>Loading/saving individual combatants as reusable character templates.</summary>
public sealed class CharacterTemplateService(ICharacterTemplateStore store)
{
    /// <summary>
    /// Loads a character template as a brand-new encounter participant: a fresh Id (via
    /// <see cref="Combatant.Duplicate"/>), distinct from whatever Id the template file was
    /// saved with, and initiative cleared. Without the fresh Id, re-importing the same file -
    /// or importing a file saved from a combatant still in the current encounter - would collide
    /// with the existing combatant's Id and get rejected by Encounter.AddCombatant.
    /// </summary>
    public async Task<Combatant> LoadFromTemplateAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var combatant = await store.LoadAsync(stream, cancellationToken);
        return combatant.Duplicate();
    }

    public Task SaveAsTemplateAsync(Combatant combatant, Stream stream, CancellationToken cancellationToken = default) =>
        store.SaveAsync(combatant, stream, cancellationToken);
}
