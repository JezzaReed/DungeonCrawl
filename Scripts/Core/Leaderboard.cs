using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace NewGameProject;

/// <summary>A single completed run, persisted to the local leaderboard.</summary>
public class RunRecord
{
    public int    Score    { get; set; }
    public int    Floor    { get; set; }
    public int    Level    { get; set; }
    public int    Kills    { get; set; }
    public string KilledBy { get; set; } = "";
    public string Date     { get; set; } = "";
}

/// <summary>
/// Local high-score table, stored as JSON in <c>user://leaderboard.json</c>
/// (the OS per-user data dir — persists across runs and survives updates).
/// </summary>
public static class Leaderboard
{
    private const string FilePath   = "user://leaderboard.json";
    public  const int    MaxEntries = 20;

    public static List<RunRecord> Load()
    {
        if (!FileAccess.FileExists(FilePath))
            return new List<RunRecord>();

        using var file = FileAccess.Open(FilePath, FileAccess.ModeFlags.Read);
        if (file == null)
            return new List<RunRecord>();

        string json = file.GetAsText();
        if (string.IsNullOrWhiteSpace(json))
            return new List<RunRecord>();

        try
        {
            return JsonSerializer.Deserialize<List<RunRecord>>(json) ?? new List<RunRecord>();
        }
        catch (JsonException e)
        {
            GD.PrintErr($"Leaderboard: failed to parse {FilePath}: {e.Message}");
            return new List<RunRecord>();
        }
    }

    private static void Save(List<RunRecord> runs)
    {
        using var file = FileAccess.Open(FilePath, FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PrintErr($"Leaderboard: cannot open {FilePath} for writing");
            return;
        }
        file.StoreString(JsonSerializer.Serialize(runs));
    }

    /// <summary>
    /// Records a run, keeping only the top <see cref="MaxEntries"/> by score.
    /// Returns the run's 1-based rank, or -1 if it didn't make the board.
    /// </summary>
    public static int Submit(RunRecord run)
    {
        var runs = Load();
        runs.Add(run);
        runs.Sort((a, b) => b.Score.CompareTo(a.Score));

        if (runs.Count > MaxEntries)
            runs.RemoveRange(MaxEntries, runs.Count - MaxEntries);

        Save(runs);

        int idx = runs.IndexOf(run);
        return idx >= 0 ? idx + 1 : -1;
    }
}
