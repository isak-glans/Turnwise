namespace Turnwise.Application.Abstractions;

/// <summary>Port for randomness, so dice rolling logic can be unit tested deterministically.</summary>
public interface IRandomSource
{
    /// <summary>Returns a random integer in the inclusive range [minInclusive, maxInclusive].</summary>
    int Next(int minInclusive, int maxInclusive);
}
