using Turnwise.Domain.Entities;

namespace Turnwise.Web.State;

/// <summary>
/// Holds the single encounter currently being run by the GM for this browser session/circuit.
/// This is presentation-layer session state, not a persisted store - persistence happens
/// explicitly via Save/Load Encounter, per the spec.
/// </summary>
public sealed class EncounterSessionState
{
    private readonly HashSet<Guid> _bulkSelectedIds = [];
    private readonly Dictionary<Guid, int> _lastSeenLogCounts = new();
    private readonly Dictionary<Guid, bool> _logExpandedStates = new();
    private readonly Dictionary<Guid, string> _rollFilters = new();
    private readonly Dictionary<Guid, string> _counterFilters = new();
    private readonly Dictionary<Guid, string> _activeTabs = new();

    public Encounter Current { get; private set; } = new();
    public Guid? SelectedCombatantId { get; private set; }

    /// <summary>Combatants checked for a bulk action (damage, condition, initiative roll) - independent of <see cref="SelectedCombatantId"/>.</summary>
    public IReadOnlySet<Guid> BulkSelectedIds => _bulkSelectedIds;

    public event Action? Changed;

    public void ReplaceEncounter(Encounter encounter)
    {
        Current = encounter;
        SelectedCombatantId = Current.Combatants.FirstOrDefault()?.Id;
        _bulkSelectedIds.Clear();
        _lastSeenLogCounts.Clear();
        _logExpandedStates.Clear();
        _rollFilters.Clear();
        _counterFilters.Clear();
        _activeTabs.Clear();
        NotifyChanged();
    }

    public void NewEncounter() => ReplaceEncounter(new Encounter());

    public void SelectCombatant(Guid? combatantId)
    {
        SelectedCombatantId = combatantId;
        NotifyChanged();
    }

    public Combatant? SelectedCombatant =>
        Current.Combatants.FirstOrDefault(c => c.Id == SelectedCombatantId);

    public bool IsBulkSelected(Guid combatantId) => _bulkSelectedIds.Contains(combatantId);

    public void ToggleBulkSelection(Guid combatantId)
    {
        if (!_bulkSelectedIds.Remove(combatantId))
        {
            _bulkSelectedIds.Add(combatantId);
        }

        NotifyChanged();
    }

    public void ClearBulkSelection()
    {
        _bulkSelectedIds.Clear();
        NotifyChanged();
    }

    public void SelectAllForBulk(IEnumerable<Guid> combatantIds)
    {
        foreach (var id in combatantIds)
        {
            _bulkSelectedIds.Add(id);
        }

        NotifyChanged();
    }

    public void RemoveFromBulkSelection(Guid combatantId)
    {
        if (_bulkSelectedIds.Remove(combatantId))
        {
            NotifyChanged();
        }
    }

    /// <summary>
    /// Unread count for a combatant's per-character log, given the log is currently collapsed
    /// for it. Lives here (not on the panel component) because the panel is recreated whenever
    /// the selected combatant changes, but "what have I already seen for combatant X" needs to
    /// survive that.
    /// </summary>
    public int UnreadLogCount(Guid combatantId, int currentTotal, bool isExpanded)
    {
        if (isExpanded)
        {
            _lastSeenLogCounts[combatantId] = currentTotal;
            return 0;
        }

        if (!_lastSeenLogCounts.TryGetValue(combatantId, out var lastSeen))
        {
            lastSeen = currentTotal;
            _lastSeenLogCounts[combatantId] = currentTotal;
        }

        return Math.Max(0, currentTotal - lastSeen);
    }

    /// <summary>
    /// Whether a combatant's per-character combat log is expanded. Lives here rather than on the
    /// panel (same reasoning as <see cref="UnreadLogCount"/>): if it were local component state,
    /// switching away and back would silently re-expand it before the GM ever saw the unread
    /// badge, defeating the point of having one. Defaults to expanded for a combatant not seen
    /// before, per the "open by default" behavior.
    /// </summary>
    public bool IsLogExpanded(Guid combatantId) =>
        !_logExpandedStates.TryGetValue(combatantId, out var expanded) || expanded;

    public void SetLogExpanded(Guid combatantId, bool expanded)
    {
        _logExpandedStates[combatantId] = expanded;
        NotifyChanged();
    }

    /// <summary>Per-character search text for the Dice Rolls list. Survives the panel being recreated on character switch, but not shared between characters.</summary>
    public string GetRollFilter(Guid combatantId) => _rollFilters.GetValueOrDefault(combatantId, "");

    public void SetRollFilter(Guid combatantId, string filter) => _rollFilters[combatantId] = filter;

    /// <summary>Per-character search text for the Counters list. Same reasoning as <see cref="GetRollFilter"/>.</summary>
    public string GetCounterFilter(Guid combatantId) => _counterFilters.GetValueOrDefault(combatantId, "");

    public void SetCounterFilter(Guid combatantId, string filter) => _counterFilters[combatantId] = filter;

    /// <summary>Which of the Counters/Conditions/Dice Rolls tabs a combatant's panel was last showing. Defaults to "counters".</summary>
    public string GetActiveTab(Guid combatantId) => _activeTabs.GetValueOrDefault(combatantId, "counters");

    public void SetActiveTab(Guid combatantId, string tab) => _activeTabs[combatantId] = tab;

    public void NotifyChanged() => Changed?.Invoke();
}
