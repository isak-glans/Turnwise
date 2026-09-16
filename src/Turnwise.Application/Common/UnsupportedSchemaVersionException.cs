namespace Turnwise.Application.Common;

/// <summary>Thrown when a loaded file's schema version is older than what this version of Turnwise supports.</summary>
public sealed class UnsupportedSchemaVersionException : Exception
{
    public int FileVersion { get; }
    public int MinSupportedVersion { get; }

    public UnsupportedSchemaVersionException(int fileVersion, int minSupportedVersion)
        : base($"File schema version {fileVersion} is older than the minimum supported version {minSupportedVersion}. " +
               "Automatic migration is not supported - please open this file with an older version of Turnwise or recreate it.")
    {
        FileVersion = fileVersion;
        MinSupportedVersion = minSupportedVersion;
    }
}
