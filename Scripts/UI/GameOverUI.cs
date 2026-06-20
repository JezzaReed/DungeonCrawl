using Godot;

namespace NewGameProject;

public partial class GameOverUI : Control
{
    public override void _Ready()
    {
        var bg = new ColorRect { Color = Color.FromHtml("#080404"), LayoutMode = 1 };
        bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        var vbox = new VBoxContainer { LayoutMode = 1 };
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
        vbox.CustomMinimumSize = new Vector2(400, 360);
        vbox.Position -= vbox.CustomMinimumSize / 2f;
        vbox.AddThemeConstantOverride("separation", 14);
        AddChild(vbox);

        var title = new Label { Text = "YOU DIED" };
        title.AddThemeFontSizeOverride("font_size", 56);
        title.AddThemeColorOverride("font_color", Color.FromHtml("#cc1111"));
        title.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(title);

        // Stats from last run
        var gm = GameManager.Instance;
        if (gm.Player != null)
        {
            var p = gm.Player;
            string stats =
                $"Killed by:      {p.KilledBy}\n" +
                $"Floor reached:  {p.Floor}\n" +
                $"Enemies slain:  {p.KillCount}\n" +
                $"Final score:    {p.Score}";

            var statsLabel = new Label { Text = stats, HorizontalAlignment = HorizontalAlignment.Center };
            statsLabel.AddThemeFontSizeOverride("font_size", 20);
            statsLabel.AddThemeColorOverride("font_color", Color.FromHtml("#aa8844"));
            vbox.AddChild(statsLabel);
        }

        // Leaderboard standing
        int rank = gm.LastRunRank;
        if (rank > 0)
        {
            bool isTop = rank == 1;
            var banner = new Label
            {
                Text = isTop ? "★ NEW HIGH SCORE! ★" : $"Leaderboard rank #{rank}",
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            banner.AddThemeFontSizeOverride("font_size", isTop ? 26 : 20);
            banner.AddThemeColorOverride("font_color", Color.FromHtml(isTop ? "#ffdd44" : "#c8a020"));
            vbox.AddChild(banner);
        }

        // Last messages
        var msgs = gm.Log.Messages;
        int start = System.Math.Max(0, msgs.Count - 3);
        var sb = new System.Text.StringBuilder();
        for (int i = start; i < msgs.Count; i++)
            sb.AppendLine(msgs[i].Text);

        var log = new Label { Text = sb.ToString(), HorizontalAlignment = HorizontalAlignment.Center };
        log.AddThemeFontSizeOverride("font_size", 14);
        log.AddThemeColorOverride("font_color", Color.FromHtml("#554433"));
        vbox.AddChild(log);

        var spacer = new Control { CustomMinimumSize = new Vector2(0, 10) };
        vbox.AddChild(spacer);

        vbox.AddChild(MakeButton("Play Again",  OnPlayAgain,  "#44cc44"));
        vbox.AddChild(MakeButton("Leaderboard", OnLeaderboard, "#c8a020"));
        vbox.AddChild(MakeButton("Quit",        OnQuit,       "#cc4444"));
    }

    private Button MakeButton(string text, System.Action onPress, string hex)
    {
        var btn = new Button { Text = text, CustomMinimumSize = new Vector2(280, 52) };
        btn.AddThemeFontSizeOverride("font_size", 22);
        btn.AddThemeColorOverride("font_color", Color.FromHtml(hex));
        btn.Pressed += onPress;
        return btn;
    }

    private void OnPlayAgain() => GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");

    private void OnLeaderboard()
    {
        LeaderboardUI.HighlightRank = GameManager.Instance.LastRunRank;
        GetTree().ChangeSceneToFile("res://Scenes/Leaderboard.tscn");
    }

    private void OnQuit() => GetTree().Quit();
}
