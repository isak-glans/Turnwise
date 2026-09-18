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

    /// <summary>Per-character search text for the Dice Rolls list. Survives the panel being recreated on character switch, but not shared between characters.</summary>
    public string GetRollFilter(Guid combatantId) => _rollFilters.GetValueOrDefault(combatantId, "");

    public void SetRollFilter(Guid combatantId, string filter) => _rollFilters[combatantId] = filter;

    /// <summary>Per-character search text for the Counters list. Same reasoning as <see cref="GetRollFilter"/>.</summary>
    public string GetCounterFilter(Guid combatantId) => _counterFilters.GetValueOrDefault(combatantId, "");

    public void SetCounterFilter(Guid combatantId, string filter) => _counterFilters[combatantId] = filter;

    /// <summary>Which of the Dice Rolls/Counters/Conditions tabs a combatant's panel was last showing. Defaults to "rolls".</summary>
    public string GetActiveTab(Guid combatantId) => _activeTabs.GetValueOrDefault(combatantId, "rolls");

    public void SetActiveTab(Guid combatantId, string tab) => _activeTabs[combatantId] = tab;

    public void NotifyChanged() => Changed?.Invoke();
}
