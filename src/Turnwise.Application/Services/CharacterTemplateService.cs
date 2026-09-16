using Turnwise.Application.Abstractions;
using Turnwise.Domain.Entities;

namespace Turnwise.Application.Services;

/// <summary>Loading/saving individual combatants as reusable character templates.</summary>
public sealed class CharacterTemplateService(ICharacterTemplateStore store)
{
    /// <summary>
    /// Loads a character template. Initiative is always cleared on import, since a stale
    /// initiative value from a previous fight should never carry over.
    /// </summary>
    public async Task<Combatant> LoadFromTemplateAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var combatant = await store.LoadAsync(stream, cancellationToken);
        combatant.ClearInitiative();
        return combatant;
    }

    public Task SaveAsTemplateAsync(Combatant combatant, Stream stream, CancellationToken cancellationToken = default) =>
        store.SaveAsync(combatant, stream, cancellationToken);
}
