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
        var fileDto = await JsonSerializer.DeserializeAsync<EncounterFileDto>(stream, SerializerOptions, cancellationToken)
            ?? throw new FormatException("Encounter file is empty or invalid.");

        if (fileDto.SchemaVersion < SchemaVersions.MinSupportedEncounterSchemaVersion)
        {
            throw new UnsupportedSchemaVersionException(fileDto.SchemaVersion, SchemaVersions.MinSupportedEncounterSchemaVersion);
        }

        var encounter = Encounter.Restore(fileDto.Id, fileDto.Name, fileDto.Round, fileDto.ActiveCombatantId, fileDto.AllowGmBulkInitiativeRoll);

        foreach (var combatantDto in fileDto.Combatants)
        {
            encounter.AddCombatant(CombatantMapper.ToDomain(combatantDto));
        }

        foreach (var logDto in fileDto.Log)
        {
            if (!Enum.TryParse<CombatLogEntryType>(logDto.Type, out var logType))
            {
                throw new FormatException($"Unknown combat log entry type '{logDto.Type}'.");
            }

            encounter.AddLogEntry(new CombatLogEntry(logType, logDto.Message, logDto.CombatantId, logDto.Timestamp, logDto.Id));
        }

        return encounter;
    }

    public async Task SaveAsync(Encounter encounter, Stream stream, CancellationToken cancellationToken = default)
    {
        var fileDto = new EncounterFileDto
        {
            SchemaVersion = SchemaVersions.CurrentEncounterSchemaVersion,
            Id = encounter.Id,
            Name = encounter.Name,
            Round = encounter.Round,
            ActiveCombatantId = encounter.ActiveCombatantId,
            AllowGmBulkInitiativeRoll = encounter.AllowGmBulkInitiativeRoll,
            Combatants = encounter.Combatants.Select(CombatantMapper.ToDto).ToList(),
            Log = encounter.Log.Select(e => new CombatLogEntryDto
            {
                Id = e.Id,
                Timestamp = e.Timestamp,
                Type = e.Type.ToString(),
                CombatantId = e.CombatantId,
                Message = e.Message
            }).ToList()
        };

        await JsonSerializer.SerializeAsync(stream, fileDto, SerializerOptions, cancellationToken);
    }
}
