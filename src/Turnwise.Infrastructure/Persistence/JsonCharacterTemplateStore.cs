using System.Text.Json;
using Turnwise.Application.Abstractions;
using Turnwise.Application.Common;
using Turnwise.Domain.Entities;
using Turnwise.Infrastructure.Persistence.Dtos;

namespace Turnwise.Infrastructure.Persistence;

/// <summary>Reads/writes a single combatant as a JSON character template file.</summary>
public sealed class JsonCharacterTemplateStore : ICharacterTemplateStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public async Task<Combatant> LoadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var fileDto = await JsonFileReader.DeserializeAsync<CharacterFileDto>(stream, SerializerOptions, "Character", cancellationToken);

        if (fileDto.SchemaVersion < SchemaVersions.MinSupportedCharacterSchemaVersion)
        {
            throw new UnsupportedSchemaVersionException(fileDto.SchemaVersion, SchemaVersions.MinSupportedCharacterSchemaVersion);
        }

        if (fileDto.Character.PortraitBase64 is { Length: > SchemaVersions.MaxPortraitBase64Length })
        {
            throw new InvalidOperationException(
                $"Portrait image is too large ({fileDto.Character.PortraitBase64.Length} base64 chars, max {SchemaVersions.MaxPortraitBase64Length}).");
        }

        return CombatantMapper.ToDomain(fileDto.Character);
    }

    public async Task SaveAsync(Combatant combatant, Stream stream, CancellationToken cancellationToken = default)
    {
        var dto = CombatantMapper.ToDto(combatant);
        if (dto.PortraitBase64 is { Length: > SchemaVersions.MaxPortraitBase64Length })
        {
            throw new InvalidOperationException(
                $"Portrait image is too large ({dto.PortraitBase64.Length} base64 chars, max {SchemaVersions.MaxPortraitBase64Length}).");
        }

        var fileDto = new CharacterFileDto
        {
            SchemaVersion = SchemaVersions.CurrentCharacterSchemaVersion,
            Character = dto
        };

        await JsonSerializer.SerializeAsync(stream, fileDto, SerializerOptions, cancellationToken);
    }
}
