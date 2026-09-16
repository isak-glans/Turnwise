using Turnwise.Domain.Enums;

namespace Turnwise.Domain.Entities;

/// <summary>
/// A single entry in the encounter's shared combat log. The per-character log shown in the
/// character info panel is a filtered view over these entries (filtered by CombatantId) -
/// it is not a separate log.
/// </summary>
public sealed class CombatLogEntry
{
    public Guid Id { get; }
    public DateTimeOffset Timestamp { get; }
    public CombatLogEntryType Type { get; }
    public Guid? CombatantId { get; }
    public string Message { get; }

    public CombatLogEntry(CombatLogEntryType type, string message, Guid? combatantId, DateTimeOffset? timestamp = null, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Log message cannot be empty.", nameof(message));
        }

        Id = id ?? Guid.NewGuid();
        Type = type;
        CombatantId = combatantId;
        Message = message;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;
    }
}
