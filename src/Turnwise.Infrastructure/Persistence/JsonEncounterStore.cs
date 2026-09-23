using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Turnwise.Application.Abstractions;
using Turnwise.Application.Common;
using Turnwise.Domain.Entities;
using Turnwise.Domain.Enums;
using Turnwise.Infrastructure.Persistence.Dtos;

namespace Turnwise.Infrastructure.Persistence;

/// <summary>Reads/writes a whole encounter (round, turn order, every combatant's full sheet) as a single JSON file.</summary>
public sealed class JsonEncounterStore : IEncounterStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public async Task<Encounter> LoadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var fileDto = await JsonFileReader.DeserializeAsync<EncounterFileDto>(stream, SerializerOptions, "Encounter", cancellationToken);

        if (fileDto.SchemaVersion < SchemaVersions.MinSupportedEncounterSchemaVersion)
        {
            throw new UnsupportedSchemaVersionException(fileDto.SchemaVersion, SchemaVersions.MinSupportedEncounterSchemaVersion);
        }

        var encounter = Encounter.Restore(fileDto.Id, fileDto.Name, fileDto.Round, fileDto.ActiveCombatantId);

        foreach (var combatantDto in fileDto.Combatants)
        {
            ResolvePortraitReference(combatantDto, fileDto.Images);
            encounter.AddCombatant(CombatantMapper.ToDomain(combatantDto));
        }

        foreach (var logDto in fileDto.Log)
        {
            if (!Enum.TryParse<CombatLogEntryType>(logDto.Type, out var logType))
            {
                throw new FormatException($"Unknown combat log entry type '{logDto.Type}'.");
            }

            var rollCategory = Enum.TryParse<NamedRollCategory>(logDto.RollCategory, out var parsedRollCategory) ? parsedRollCategory : (NamedRollCategory?)null;
            encounter.AddLogEntry(new CombatLogEntry(logType, logDto.Message, logDto.CombatantId, logDto.Timestamp, logDto.Id, logDto.Result, rollCategory));
        }

        return encounter;
    }

    public async Task SaveAsync(Encounter encounter, Stream stream, CancellationToken cancellationToken = default)
    {
        var images = new Dictionary<string, string>();
        var combatantDtos = encounter.Combatants
            .Select(CombatantMapper.ToDto)
            .ToList();

        foreach (var combatantDto in combatantDtos)
        {
            ExtractPortraitReference(combatantDto, images);
        }

        var fileDto = new EncounterFileDto
        {
            SchemaVersion = SchemaVersions.CurrentEncounterSchemaVersion,
            Id = encounter.Id,
            Name = encounter.Name,
            Round = encounter.Round,
            ActiveCombatantId = encounter.ActiveCombatantId,
            Combatants = combatantDtos,
            Images = images,
            Log = encounter.Log.Select(e => new CombatLogEntryDto
            {
                Id = e.Id,
                Timestamp = e.Timestamp,
                Type = e.Type.ToString(),
                CombatantId = e.CombatantId,
                Message = e.Message,
                Result = e.Result,
                RollCategory = e.RollCategory?.ToString()
            }).ToList()
        };

        await JsonSerializer.SerializeAsync(stream, fileDto, SerializerOptions, cancellationToken);
    }

    /// <summary>Moves a combatant's inline portrait into the shared, content-hashed image pool - several combatants with the same art (e.g. a pack of goblins) end up pointing at the same entry instead of each embedding their own copy.</summary>
    private static void ExtractPortraitReference(CombatantDto combatantDto, Dictionary<string, string> images)
    {
        if (string.IsNullOrEmpty(combatantDto.PortraitBase64))
        {
            return;
        }

        var imageId = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(combatantDto.PortraitBase64)));
        images[imageId] = combatantDto.PortraitBase64;
        combatantDto.PortraitImageId = imageId;
        combatantDto.PortraitBase64 = null;
    }

    private static void ResolvePortraitReference(CombatantDto combatantDto, IReadOnlyDictionary<string, string> images)
    {
        if (combatantDto.PortraitImageId is not { } imageId)
        {
            return;
        }

        if (!images.TryGetValue(imageId, out var base64))
        {
            throw new FormatException($"Encounter file references unknown image '{imageId}'.");
        }

        combatantDto.PortraitBase64 = base64;
        combatantDto.PortraitImageId = null;
    }
}
