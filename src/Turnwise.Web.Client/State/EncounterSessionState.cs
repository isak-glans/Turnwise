using Turnwise.Domain.Entities;

namespace Turnwise.Web.Client.State;

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
    private Encounter? _undoSnapshot;

    public Encounter Current { get; private set; } = new();
    public Guid? SelectedCombatantId { get; private set; }

    /// <summary>Whether the initiative list's bulk-select checkboxes are shown. Off by default so they don't clutter the list until the GM asks for them.</summary>
    public bool SelectModeOn { get; private set; }

    /// <summary>Whether a snapshot is available to restore via <see cref="Undo"/>.</summary>
    public bool CanUndo => _undoSnapshot is not null;

    /// <summary>Combatants checked for a bulk action (damage, condition, initiative roll) - independent of <see cref="SelectedCombatantId"/>.</summary>
    public IReadOnlySet<Guid> BulkSelectedIds => _bulkSelectedIds;

    public event Action? Changed;

    public void ReplaceEncounter(Encounter encounter)
    {
        Current = encounter;
        SelectedCombatantId = Current.Combatants.FirstOrDefault()?.Id;
        SelectModeOn = false;
        _bulkSelectedIds.Clear();
        _rollFilters.Clear();
        _counterFilters.Clear();
        _activeTabs.Clear();
        _undoSnapshot = null;
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

    public void ToggleSelectMode()
    {
        SelectModeOn = !SelectModeOn;
        if (!SelectModeOn)
        {
            _bulkSelectedIds.Clear();
        }

        NotifyChanged();
    }

    /// <summary>Turns select mode off and clears whatever was picked - what the bulk-actions panel's Close button does.</summary>
    public void ExitSelectMode()
    {
        SelectModeOn = false;
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

    /// <summary>Which of the Details/Dice Rolls/Counters/Conditions/Notes tabs a combatant's panel was last showing. Defaults to Dice Rolls.</summary>
    public string GetActiveTab(Guid combatantId) => _activeTabs.GetValueOrDefault(combatantId, "rolls");

    public void SetActiveTab(Guid combatantId, string tab) => _activeTabs[combatantId] = tab;

    /// <summary>
    /// Snapshots the current encounter so a following <see cref="Undo"/> can restore it. Call
    /// this immediately before an in-combat HP/condition/counter/dice-roll action. A single
    /// slot, not a stack: capturing again overwrites whatever was captured before, so only the
    /// most recent action can be undone.
    /// </summary>
    public void CaptureUndoSnapshot() => _undoSnapshot = Current.Clone();

    /// <summary>Restores the last captured snapshot, if any. A no-op if nothing is captured.</summary>
    public void Undo()
    {
        if (_undoSnapshot is null)
        {
            return;
        }

        Current = _undoSnapshot;
        _undoSnapshot = null;
        NotifyChanged();
    }

    public void NotifyChanged() => Changed?.Invoke();
}
