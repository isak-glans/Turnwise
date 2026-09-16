namespace Turnwise.Domain.Entities;

/// <summary>
/// An encounter: the combatants in it (list order = turn order), the current round,
/// whose turn is active, and the shared combat log.
/// </summary>
public sealed class Encounter
{
    private readonly List<Combatant> _combatants = [];
    private readonly List<CombatLogEntry> _log = [];

    public Guid Id { get; }
    public string Name { get; set; }
    public int Round { get; private set; } = 1;
    public Guid? ActiveCombatantId { get; private set; }

    /// <summary>Whether the GM can roll initiative for every combatant at once, instead of one at a time.</summary>
    public bool AllowGmBulkInitiativeRoll { get; set; }

    /// <summary>Turn order - list position is the order of play. Reordered via drag-and-drop or auto-sort.</summary>
    public IReadOnlyList<Combatant> Combatants => _combatants;

    public IReadOnlyList<CombatLogEntry> Log => _log;

    public Encounter(string name = "New Encounter", Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        Name = name;
    }

    /// <summary>Reconstructs an encounter with exact saved state (used when loading from a file). Combatants and log entries are added separately.</summary>
    public static Encounter Restore(Guid id, string name, int round, Guid? activeCombatantId, bool allowGmBulkInitiativeRoll) =>
        new(name, id)
        {
            Round = round,
            ActiveCombatantId = activeCombatantId,
            AllowGmBulkInitiativeRoll = allowGmBulkInitiativeRoll
        };

    public void AddCombatant(Combatant combatant, int? atIndex = null)
    {
        if (_combatants.Any(c => c.Id == combatant.Id))
        {
            throw new InvalidOperationException($"Combatant {combatant.Id} is already in this encounter.");
        }

        if (atIndex is { } index && index >= 0 && index < _combatants.Count)
        {
            _combatants.Insert(index, combatant);
        }
        else
        {
            _combatants.Add(combatant);
        }
    }

    public void RemoveCombatant(Guid combatantId)
    {
        _combatants.RemoveAll(c => c.Id == combatantId);
        if (ActiveCombatantId == combatantId)
        {
            ActiveCombatantId = _combatants.Count > 0 ? _combatants[0].Id : null;
        }
    }

    /// <summary>Manual drag-and-drop reordering: moves the combatant at <paramref name="fromIndex"/> to <paramref name="toIndex"/>.</summary>
    public void Reorder(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _combatants.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(fromIndex));
        }

        if (toIndex < 0 || toIndex >= _combatants.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(toIndex));
        }

        var combatant = _combatants[fromIndex];
        _combatants.RemoveAt(fromIndex);
        _combatants.Insert(toIndex, combatant);
    }

    /// <summary>Re-sorts the whole list by initiative value, highest first. Combatants without initiative sink to the bottom.</summary>
    public void AutoSortByInitiative()
    {
        var sorted = _combatants
            .OrderByDescending(c => c.Initiative.HasValue)
            .ThenByDescending(c => c.Initiative ?? int.MinValue)
            .ToList();

        _combatants.Clear();
        _combatants.AddRange(sorted);
    }

    /// <summary>Advances to the next combatant in turn order, incrementing the round when it wraps back to the top.</summary>
    public void NextTurn()
    {
        if (_combatants.Count == 0)
        {
            ActiveCombatantId = null;
            return;
        }

        var currentIndex = ActiveCombatantId is null
            ? -1
            : _combatants.FindIndex(c => c.Id == ActiveCombatantId);

        var nextIndex = currentIndex + 1;
        if (nextIndex >= _combatants.Count)
        {
            nextIndex = 0;
            Round++;
        }

        ActiveCombatantId = _combatants[nextIndex].Id;
    }

    public void AddLogEntry(CombatLogEntry entry) => _log.Add(entry);

    public IEnumerable<CombatLogEntry> GetLogForCombatant(Guid combatantId) =>
        _log.Where(e => e.CombatantId == combatantId);
}
