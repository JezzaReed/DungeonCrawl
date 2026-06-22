using System;
using Godot;

namespace NewGameProject;

/// <summary>Small shared factory for the menu-style buttons used across UI screens.</summary>
internal static class UiBuilder
{
    public static Button MakeButton(string text, Action onPressed, string hex,
                                    int width = 280, int height = 52, int fontSize = 22)
    {
        var btn = new Button { Text = text, CustomMinimumSize = new Vector2(width, height) };
        btn.AddThemeFontSizeOverride("font_size", fontSize);
        btn.AddThemeColorOverride("font_color", Color.FromHtml(hex));
        btn.Pressed += onPressed;
        return btn;
    }
}
