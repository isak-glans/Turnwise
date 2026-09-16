using Turnwise.Domain.Enums;
using Turnwise.Domain.ValueObjects;

namespace Turnwise.Domain.Entities;

/// <summary>
/// A single combatant in an encounter. The only assumptions the system makes about any
/// TRPG are that a combatant has HP and Initiative - everything else is generic.
/// </summary>
public sealed class Combatant
{
    private readonly List<NamedRoll> _namedRolls = [];
    private readonly List<Counter> _counters = [];
    private readonly List<Condition> _conditions = [];

    public Guid Id { get; }
    public string Name { get; set; }
    public CombatantType Type { get; set; }
    public string? PortraitBase64 { get; set; }

    public int MaxHp { get; private set; }
    public bool MaxHpLocked { get; set; }
    public int CurrentHp { get; private set; }

    public string? InitiativeFormula { get; set; }
    public int? Initiative { get; private set; }
    public bool InitiativeLocked { get; set; }

    public IReadOnlyList<NamedRoll> NamedRolls => _namedRolls;
    public IReadOnlyList<Counter> Counters => _counters;
    public IReadOnlyList<Condition> Conditions => _conditions;

    public bool IsDefeated => CurrentHp <= 0;

    public Combatant(string name, CombatantType type, int maxHp, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Combatant name cannot be empty.", nameof(name));
        }

        if (maxHp < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxHp), "Max HP cannot be negative.");
        }

        Id = id ?? Guid.NewGuid();
        Name = name;
        Type = type;
        MaxHp = maxHp;
        CurrentHp = maxHp;
        MaxHpLocked = true;
        InitiativeLocked = true;
    }

    /// <summary>Reconstructs a combatant with exact saved state (used when loading from a file). Named rolls, counters and conditions are added separately.</summary>
    public static Combatant Restore(
        Guid id,
        string name,
        CombatantType type,
        int maxHp,
        int currentHp,
        bool maxHpLocked,
        string? portraitBase64,
        string? initiativeFormula,
        int? initiative,
        bool initiativeLocked = false)
    {
        var combatant = new Combatant(name, type, maxHp, id)
        {
            MaxHpLocked = maxHpLocked,
            PortraitBase64 = portraitBase64,
            InitiativeFormula = initiativeFormula,
            InitiativeLocked = initiativeLocked,
            CurrentHp = Math.Clamp(currentHp, 0, maxHp)
        };
        combatant.SetInitiative(initiative);
        return combatant;
    }

    /// <summary>
    /// Creates an independent copy with a new identity - same stats, rolls, counters and
    /// conditions, but a cleared initiative (it hasn't acted yet, just like a freshly
    /// imported character). Useful for spinning up several identical monsters.
    /// </summary>
    public Combatant Duplicate()
    {
        var clone = Restore(Guid.NewGuid(), Name, Type, MaxHp, CurrentHp, MaxHpLocked, PortraitBase64, InitiativeFormula, null, InitiativeLocked);

        foreach (var roll in _namedRolls)
        {
            clone.AddNamedRoll(roll.Name, roll.Formula);
        }

        foreach (var counter in _counters)
        {
            clone.AddCounter(counter.Name, counter.Current, counter.Max, counter.ShowBar);
        }

        foreach (var condition in _conditions)
        {
            clone.AddCondition(condition.Name);
        }

        return clone;
    }

    /// <summary>Applies a signed HP delta (negative = damage, positive = healing), clamped to [0, MaxHp].</summary>
    /// <returns>The HP change that was actually applied, after clamping.</returns>
    public int ApplyHpDelta(int delta)
    {
        var before = CurrentHp;
        CurrentHp = Math.Clamp(CurrentHp + delta, 0, MaxHp);
        return CurrentHp - before;
    }

    public void SetMaxHp(int maxHp)
    {
        if (maxHp < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxHp), "Max HP cannot be negative.");
        }

        MaxHp = maxHp;
        CurrentHp = Math.Clamp(CurrentHp, 0, MaxHp);
    }

    public void SetInitiative(int? value) => Initiative = value;

    /// <summary>Clears initiative. Used when importing a saved character template into a new encounter.</summary>
    public void ClearInitiative() => Initiative = null;

    public NamedRoll AddNamedRoll(string name, DiceFormula formula, Guid? id = null)
    {
        var roll = new NamedRoll(name, formula, id);
        _namedRolls.Add(roll);
        return roll;
    }

    public void RemoveNamedRoll(Guid rollId) => _namedRolls.RemoveAll(r => r.Id == rollId);

    public Counter AddCounter(string name, int current, int max, bool showBar = true, Guid? id = null)
    {
        var counter = new Counter(name, current, max, showBar, id);
        _counters.Add(counter);
        return counter;
    }

    public void RemoveCounter(Guid counterId) => _counters.RemoveAll(c => c.Id == counterId);

    public Condition AddCondition(string name, Guid? id = null)
    {
        var condition = new Condition(name, id);
        _conditions.Add(condition);
        return condition;
    }

    public void RemoveCondition(Guid conditionId) => _conditions.RemoveAll(c => c.Id == conditionId);
}
