using System.Diagnostics.CodeAnalysis;

namespace Turnwise.Domain.Entities;

/// <summary>A generic, system-agnostic numeric tracker (e.g. Superiority Dice, Exhaustion, Ki points, ammo).</summary>
public sealed class Counter
{
    public const int MaxNameLength = 100;

    private string _name;

    public Guid Id { get; }

    /// <summary>Silently truncated to <see cref="MaxNameLength"/> rather than rejected, since this can be set on every keystroke while editing.</summary>
    public string Name
    {
        get => _name;
        [MemberNotNull(nameof(_name))]
        set => _name = Truncate(value);
    }

    public int Current { get; private set; }
    public int Max { get; private set; }
    public bool ShowBar { get; set; }

    public Counter(string name, int current, int max, bool showBar = true, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Counter name cannot be empty.", nameof(name));
        }

        if (max < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(max), "Max value cannot be negative.");
        }

        Id = id ?? Guid.NewGuid();
        Name = name;
        Max = max;
        Current = Math.Clamp(current, 0, max);
        ShowBar = showBar;
    }

    public void Adjust(int delta)
    {
        Current = Math.Clamp(Current + delta, 0, Max);
    }

    public void SetMax(int max)
    {
        if (max < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(max), "Max value cannot be negative.");
        }

        Max = max;
        Current = Math.Clamp(Current, 0, Max);
    }

    private static string Truncate(string? value)
    {
        var text = value ?? "";
        return text.Length > MaxNameLength ? text[..MaxNameLength] : text;
    }
}
