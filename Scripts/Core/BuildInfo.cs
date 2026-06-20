namespace NewGameProject;

/// <summary>
/// Build identity. The <see cref="Number"/> value is replaced by CI at build
/// time (GitHub Actions run number); locally it stays "dev".
/// </summary>
public static class BuildInfo
{
    public const string Number = "dev";
}
