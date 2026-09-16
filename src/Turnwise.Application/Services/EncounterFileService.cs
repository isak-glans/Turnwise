using Turnwise.Application.Abstractions;
using Turnwise.Domain.Entities;

namespace Turnwise.Application.Services;

/// <summary>Loading/saving a whole encounter (round, turn order, every combatant's full sheet) as a single file.</summary>
public sealed class EncounterFileService(IEncounterStore store)
{
    public Task<Encounter> LoadEncounterAsync(Stream stream, CancellationToken cancellationToken = default) =>
        store.LoadAsync(stream, cancellationToken);

    public Task SaveEncounterAsync(Encounter encounter, Stream stream, CancellationToken cancellationToken = default) =>
        store.SaveAsync(encounter, stream, cancellationToken);
}
