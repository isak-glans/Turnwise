using Turnwise.Domain.Entities;

namespace Turnwise.Web.State;

/// <summary>
/// Holds the single encounter currently being run by the GM for this browser session/circuit.
/// This is presentation-layer session state, not a persisted store - persistence happens
/// explicitly via Save/Load Encounter, per the spec.
/// </summary>
public sealed class EncounterSessionState
{
    public Encounter Current { get; private set; } = new();
    public Guid? SelectedCombatantId { get; private set; }

    public event Action? Changed;

    public void ReplaceEncounter(Encounter encounter)
    {
        Current = encounter;
        SelectedCombatantId = Current.Combatants.FirstOrDefault()?.Id;
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

    public void NotifyChanged() => Changed?.Invoke();
}
