using System.Diagnostics.CodeAnalysis;
using Turnwise.Domain.ValueObjects;

namespace Turnwise.Domain.Entities;

/// <summary>
/// A single combatant in an encounter. The only assumptions the system makes about any
/// TRPG are that a combatant has HP and Initiative - everything else is generic.
/// </summary>
public sealed class Combatant
{
    /// <summary>Keeps the name usable as part of a save-file name - well under filesystem path-component limits.</summary>
    public const int MaxNameLength = 100;

    private readonly List<NamedRoll> _namedRolls = [];
    private readonly List<Counter> _counters = [];
    private readonly List<Condition> _conditions = [];
    private string _name;

    public Guid Id { get; }

    /// <summary>Silently truncated to <see cref="MaxNameLength"/> rather than rejected, since this is set on every keystroke while the GM is typing.</summary>
    public string Name
    {
        get => _name;
        [MemberNotNull(nameof(_name))]
        set => _name = Truncate(value);
    }

    public string? PortraitBase64 { get; set; }

    public int MaxHp { get; private set; }
    public bool MaxHpLocked { get; set; }
    public int CurrentHp { get; private set; }
    public int? ArmorClass { get; private set; }

    public string? InitiativeFormula { get; set; }
    public int? Initiative { get; private set; }
    public bool InitiativeLocked { get; set; }

    public IReadOnlyList<NamedRoll> NamedRolls => _namedRolls;
    public IReadOnlyList<Counter> Counters => _counters;
    public IReadOnlyList<Condition> Conditions => _conditions;

    public bool IsDefeated => CurrentHp <= 0;

    public Combatant(string name, int maxHp, Guid? id = null)
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
        MaxHp = maxHp;
        CurrentHp = maxHp;
        MaxHpLocked = true;
        InitiativeLocked = true;
    }

    private static string Truncate(string? value)
    {
        var text = value ?? "";
        return text.Length > MaxNameLength ? text[..MaxNameLength] : text;
    }

    /// <summary>Reconstructs a combatant with exact saved state (used when loading from a file). Named rolls, counters and conditions are added separately.</summary>
    public static Combatant Restore(
        Guid id,
        string name,
        int maxHp,
        int currentHp,
        bool maxHpLocked,
        string? portraitBase64,
        string? initiativeFormula,
        int? initiative,
        bool initiativeLocked = false,
        int? armorClass = null)
    {
        var combatant = new Combatant(name, maxHp, id)
        {
            MaxHpLocked = maxHpLocked,
            PortraitBase64 = portraitBase64,
            InitiativeFormula = initiativeFormula,
            InitiativeLocked = initiativeLocked,
            CurrentHp = Math.Clamp(currentHp, 0, maxHp)
        };
        combatant.SetInitiative(initiative);
        combatant.SetArmorClass(armorClass);
        return combatant;
    }

    /// <summary>
    /// Creates an independent copy with a new identity - same stats, rolls, counters and
    /// conditions, but a cleared initiative (it hasn't acted yet, just like a freshly
    /// imported character). Useful for spinning up several identical monsters.
    /// </summary>
    public Combatant Duplicate()
    {
        var clone = Restore(Guid.NewGuid(), Name, MaxHp, CurrentHp, MaxHpLocked, PortraitBase64, InitiativeFormula, null, InitiativeLocked, ArmorClass);

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
        // Widen to long first: CurrentHp + delta can overflow int when delta comes from
        // unbounded free-text input (e.g. int.MaxValue), which would otherwise wrap around
        // to a huge negative number before Clamp ever sees it.
        var target = (long)CurrentHp + delta;
        CurrentHp = (int)Math.Clamp(target, 0, MaxHp);
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

    public void SetArmorClass(int? value)
    {
        if (value is { } ac && ac < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Armor Class cannot be negative.");
        }

        ArmorClass = value;
    }

    /// <summary>Clears initiative. Used when importing a saved character template into a new encounter.</summary>
    public void ClearInitiative() => Initiative = null;

    public NamedRoll AddNamedRoll(string name, DiceFormula formula, Guid? id = null)
    {
        var roll = new NamedRoll(name, formula, id);
        _namedRolls.Add(roll);
        return roll;
    }

    public void RemoveNamedRoll(Guid rollId) => _namedRolls.RemoveAll(r => r.Id == rollId);

    /// <summary>Manual drag-and-drop reordering of the dice roll list.</summary>
    public void ReorderNamedRoll(int fromIndex, int toIndex) => Move(_namedRolls, fromIndex, toIndex);

    /// <summary>Adds a copy of the given roll right after it in the list.</summary>
    public NamedRoll DuplicateNamedRoll(Guid rollId)
    {
        var index = _namedRolls.FindIndex(r => r.Id == rollId);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Named roll {rollId} not found.");
        }

        var original = _namedRolls[index];
        var clone = new NamedRoll(original.Name, original.Formula);
        _namedRolls.Insert(index + 1, clone);
        return clone;
    }

    public Counter AddCounter(string name, int current, int max, bool showBar = true, Guid? id = null)
    {
        var counter = new Counter(name, current, max, showBar, id);
        _counters.Add(counter);
        return counter;
    }

    public void RemoveCounter(Guid counterId) => _counters.RemoveAll(c => c.Id == counterId);

    /// <summary>Manual drag-and-drop reordering of the counter list.</summary>
    public void ReorderCounter(int fromIndex, int toIndex) => Move(_counters, fromIndex, toIndex);

    /// <summary>Adds a copy of the given counter right after it in the list.</summary>
    public Counter DuplicateCounter(Guid counterId)
    {
        var index = _counters.FindIndex(c => c.Id == counterId);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Counter {counterId} not found.");
        }

        var original = _counters[index];
        var clone = new Counter(original.Name, original.Current, original.Max, original.ShowBar);
        _counters.Insert(index + 1, clone);
        return clone;
    }

    private static void Move<T>(List<T> list, int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= list.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(fromIndex));
        }

        if (toIndex < 0 || toIndex >= list.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(toIndex));
        }

        var item = list[fromIndex];
        list.RemoveAt(fromIndex);
        list.Insert(toIndex, item);
    }

    public Condition AddCondition(string name, Guid? id = null)
    {
        var condition = new Condition(name, id);
        _conditions.Add(condition);
        return condition;
    }

    public void RemoveCondition(Guid conditionId) => _conditions.RemoveAll(c => c.Id == conditionId);
}
