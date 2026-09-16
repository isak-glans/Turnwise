using Turnwise.Domain.Entities;

namespace Turnwise.Application.Abstractions;

/// <summary>Port for reading/writing a single combatant as a reusable character template file.</summary>
public interface ICharacterTemplateStore
{
    Task<Combatant> LoadAsync(Stream stream, CancellationToken cancellationToken = default);

    Task SaveAsync(Combatant combatant, Stream stream, CancellationToken cancellationToken = default);
}
