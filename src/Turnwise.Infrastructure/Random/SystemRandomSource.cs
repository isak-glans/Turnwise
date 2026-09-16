using Turnwise.Application.Abstractions;

namespace Turnwise.Infrastructure.Random;

/// <summary>Thin wrapper over <see cref="System.Random.Shared"/> so dice rolling can be tested with a fake instead.</summary>
public sealed class SystemRandomSource : IRandomSource
{
    public int Next(int minInclusive, int maxInclusive) =>
        System.Random.Shared.Next(minInclusive, maxInclusive + 1);
}
