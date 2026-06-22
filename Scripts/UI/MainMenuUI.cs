using Godot;

namespace NewGameProject;

public partial class MainMenuUI : Control
{
    public override void _Ready()
    {
        // Dark background
        var bg = new ColorRect { Color = Color.FromHtml("#0a0806"), LayoutMode = 1 };
        bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        // Centered VBox
        var vbox = new VBoxContainer { LayoutMode = 1 };
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
        vbox.CustomMinimumSize = new Vector2(340, 320);
        vbox.Position -= vbox.CustomMinimumSize / 2f;
        vbox.AddThemeConstantOverride("separation", 18);
        AddChild(vbox);

        // Title
        var title = new Label { Text = "DUNGEON CRAWL" };
        title.AddThemeFontSizeOverride("font_size", 42);
        title.AddThemeColorOverride("font_color", Color.FromHtml("#c8a020"));
        title.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(title);

        var subtitle = new Label { Text = "A Roguelike" };
        subtitle.AddThemeFontSizeOverride("font_size", 18);
        subtitle.AddThemeColorOverride("font_color", Color.FromHtml("#886622"));
        subtitle.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(subtitle);

        // Spacer
        var spacer = new Control { CustomMinimumSize = new Vector2(0, 20) };
        vbox.AddChild(spacer);

        // Buttons
        vbox.AddChild(UiBuilder.MakeButton("New Game",    OnNewGame,    "#44cc44"));
        vbox.AddChild(UiBuilder.MakeButton("High Scores", OnHighScores, "#c8a020"));
        vbox.AddChild(UiBuilder.MakeButton("Quit",        OnQuit,       "#cc4444"));

        // Controls hint
        var hint = new Label
        {
            Text = "Arrow keys / WASD / Numpad to move\nBump enemies to attack · [Q] drink potion · [.] wait · [Esc] pause",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        hint.AddThemeFontSizeOverride("font_size", 13);
        hint.AddThemeColorOverride("font_color", Color.FromHtml("#555544"));
        vbox.AddChild(hint);

        // Build number, pinned to the bottom-left corner
        var build = new Label
        {
            Text = $"Build {BuildInfo.Number}",
            LayoutMode = 1,
        };
        build.SetAnchorsAndOffsetsPreset(LayoutPreset.BottomLeft);
        build.OffsetLeft = 8;
        build.OffsetTop = -24;
        build.AddThemeFontSizeOverride("font_size", 12);
        build.AddThemeColorOverride("font_color", Color.FromHtml("#444433"));
        AddChild(build);
    }

    private void OnNewGame()    => GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
    private void OnHighScores() => GetTree().ChangeSceneToFile("res://Scenes/Leaderboard.tscn");
    private void OnQuit()       => GetTree().Quit();
}
