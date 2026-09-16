using Turnwise.Domain.Entities;

namespace Turnwise.Application.Abstractions;

/// <summary>Port for reading/writing a full encounter (round, turn order, every combatant) as a single file.</summary>
public interface IEncounterStore
{
    Task<Encounter> LoadAsync(Stream stream, CancellationToken cancellationToken = default);

    Task SaveAsync(Encounter encounter, Stream stream, CancellationToken cancellationToken = default);
}
