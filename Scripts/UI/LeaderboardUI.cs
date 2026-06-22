using System.Collections.Generic;
using Godot;

namespace NewGameProject;

public partial class LeaderboardUI : Control
{
    // Set by GameOverUI so we can highlight the run the player just finished.
    public static int HighlightRank = -1;

    public override void _Ready()
    {
        var bg = new ColorRect { Color = Color.FromHtml("#0a0806"), LayoutMode = 1 };
        bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        var vbox = new VBoxContainer { LayoutMode = 1 };
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
        vbox.CustomMinimumSize = new Vector2(560, 480);
        vbox.Position -= vbox.CustomMinimumSize / 2f;
        vbox.AddThemeConstantOverride("separation", 12);
        AddChild(vbox);

        var title = new Label { Text = "HIGH SCORES" };
        title.AddThemeFontSizeOverride("font_size", 40);
        title.AddThemeColorOverride("font_color", Color.FromHtml("#c8a020"));
        title.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(title);

        var runs = Leaderboard.Load();

        if (runs.Count == 0)
        {
            var empty = new Label { Text = "No runs recorded yet.\nDescend and die gloriously." };
            empty.HorizontalAlignment = HorizontalAlignment.Center;
            empty.AddThemeFontSizeOverride("font_size", 18);
            empty.AddThemeColorOverride("font_color", Color.FromHtml("#776655"));
            vbox.AddChild(empty);
        }
        else
        {
            vbox.AddChild(MakeRow("#", "SCORE", "FLOOR", "LVL", "KILLS", "DATE", "#998866", header: true));
            for (int i = 0; i < runs.Count; i++)
            {
                var r = runs[i];
                int rank = i + 1;
                string color = rank == HighlightRank ? "#ffdd44" : "#aa8844";
                vbox.AddChild(MakeRow(
                    rank.ToString(), r.Score.ToString(), r.Floor.ToString(),
                    r.Level.ToString(), r.Kills.ToString(), r.Date, color));
            }
        }

        var spacer = new Control { CustomMinimumSize = new Vector2(0, 10) };
        vbox.AddChild(spacer);

        vbox.AddChild(UiBuilder.MakeButton("Back", OnBack, "#cccccc", height: 48, fontSize: 20));
    }

    private void OnBack()
    {
        HighlightRank = -1;
        GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
    }

    private static readonly int[] ColWidths = { 36, 110, 70, 50, 70, 150 };

    private HBoxContainer MakeRow(string rank, string score, string floor,
                                  string lvl, string kills, string date,
                                  string hex, bool header = false)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 8);
        var cols = new List<string> { rank, score, floor, lvl, kills, date };
        var aligns = new[]
        {
            HorizontalAlignment.Right, HorizontalAlignment.Right, HorizontalAlignment.Center,
            HorizontalAlignment.Center, HorizontalAlignment.Center, HorizontalAlignment.Left,
        };

        for (int c = 0; c < cols.Count; c++)
        {
            var cell = new Label
            {
                Text = cols[c],
                CustomMinimumSize = new Vector2(ColWidths[c], 0),
                HorizontalAlignment = aligns[c],
            };
            cell.AddThemeFontSizeOverride("font_size", header ? 14 : 17);
            cell.AddThemeColorOverride("font_color", Color.FromHtml(hex));
            row.AddChild(cell);
        }
        return row;
    }
}
