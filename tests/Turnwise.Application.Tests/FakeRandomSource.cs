using Turnwise.Application.Abstractions;

namespace Turnwise.Application.Tests;

/// <summary>Deterministic stand-in for randomness: replays a fixed sequence, then repeats the last value.</summary>
public sealed class FakeRandomSource(params int[] values) : IRandomSource
{
    private int _index;

    public int Next(int minInclusive, int maxInclusive)
    {
        var value = values[Math.Min(_index, values.Length - 1)];
        _index++;
        return Math.Clamp(value, minInclusive, maxInclusive);
    }
}
