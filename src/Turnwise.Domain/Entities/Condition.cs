namespace Turnwise.Domain.Entities;

/// <summary>
/// A status condition currently affecting a combatant (e.g. Poisoned, Prone, Stunned).
/// Intentionally just a name - no built-in duration tracking. A GM who needs to count down
/// rounds remaining can pair a condition with a <see cref="Counter"/>.
/// </summary>
public sealed class Condition
{
    public Guid Id { get; }
    public string Name { get; set; }

    public Condition(string name, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Condition name cannot be empty.", nameof(name));
        }

        Id = id ?? Guid.NewGuid();
        Name = name;
    }
}
