using Godot;

namespace NewGameProject;

/// <summary>
/// Pause overlay opened with [Esc]. Resume continues the run; Resign ends it,
/// banking the current score on the leaderboard exactly as a death would.
/// </summary>
public partial class PauseMenuUI : Control
{
    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;

        var bg = new ColorRect { Color = new Color(0, 0, 0, 0.72f), LayoutMode = 1 };
        bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        var vbox = new VBoxContainer { LayoutMode = 1 };
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
        vbox.CustomMinimumSize = new Vector2(360, 280);
        vbox.Position -= vbox.CustomMinimumSize / 2f;
        vbox.AddThemeConstantOverride("separation", 18);
        AddChild(vbox);

        var title = new Label { Text = "PAUSED" };
        title.AddThemeFontSizeOverride("font_size", 44);
        title.AddThemeColorOverride("font_color", Color.FromHtml("#c8a020"));
        title.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(title);

        var spacer = new Control { CustomMinimumSize = new Vector2(0, 8) };
        vbox.AddChild(spacer);

        vbox.AddChild(UiBuilder.MakeButton("Resume", OnResume, "#44cc44"));
        vbox.AddChild(UiBuilder.MakeButton("Resign", OnResign, "#cc4444"));

        var hint = new Label
        {
            Text = "Resigning banks your score on the leaderboard.",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        hint.AddThemeFontSizeOverride("font_size", 13);
        hint.AddThemeColorOverride("font_color", Color.FromHtml("#776655"));
        vbox.AddChild(hint);

        GameManager.Instance.StateChanged += Refresh;
        Refresh();
    }

    public override void _ExitTree()
    {
        GameManager.Instance.StateChanged -= Refresh;
    }

    private void Refresh() => Visible = GameManager.Instance.IsPaused;

    private void OnResume() => GameManager.Instance.ResumeGame();

    private void OnResign()
    {
        GameManager.Instance.Resign();
        GetTree().ChangeSceneToFile("res://Scenes/GameOver.tscn");
    }
}
