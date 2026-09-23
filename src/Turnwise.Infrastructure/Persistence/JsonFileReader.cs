using System.Text.Json;

namespace Turnwise.Infrastructure.Persistence;

/// <summary>Wraps JSON deserialization with GM-readable error messages - a raw <see cref="JsonException"/>
/// message ("The JSON value could not be converted...") doesn't tell a GM (or an AI fixing a
/// file on their behalf) where the problem is or what to do about it.</summary>
internal static class JsonFileReader
{
    public static async Task<T> DeserializeAsync<T>(
        Stream stream, JsonSerializerOptions options, string fileKind, CancellationToken cancellationToken)
    {
        try
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken)
                ?? throw new FormatException($"{fileKind} file is empty - there's no JSON object in it to load.");
        }
        catch (JsonException ex)
        {
            // Deliberately not including ex.Message here: under the trimmed WASM runtime, some
            // low-level parse failures return a raw internal resource key (e.g. "ZeroDepthAtEnd")
            // instead of the readable sentence a normal .NET process gets for the same input -
            // not something to show a GM. Path/LineNumber are plain data, so they're reliable.
            var location = ex.Path is { } path && path != "$"
                ? $" at {path}"
                : ex.LineNumber is { } line ? $" near line {line + 1}" : "";

            throw new FormatException(
                $"{fileKind} file isn't valid JSON{location}. " +
                "Check for a missing comma, quote or bracket near that spot, or ask an AI assistant to fix the file and re-export it.",
                ex);
        }
    }
}
