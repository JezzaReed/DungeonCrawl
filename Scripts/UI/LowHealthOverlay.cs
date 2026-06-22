using System;
using Godot;

namespace NewGameProject;

/// <summary>
/// Pulsing red edge-vignette shown while the player is below
/// <see cref="Threshold"/> HP. Purely cosmetic — never blocks input.
/// </summary>
public partial class LowHealthOverlay : Control
{
    private const float Threshold = 0.20f;

    private TextureRect _vignette = null!;
    private bool   _active;
    private double _time;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;

        // Radial gradient: clear centre fading to a thin red edge band
        var gradient = new Gradient
        {
            Offsets = new[] { 0f, 0.75f, 1f },
            Colors  = new[]
            {
                new Color(0.8f, 0, 0, 0f),
                new Color(0.8f, 0, 0, 0f),
                new Color(0.8f, 0, 0, 0.4f),
            },
        };
        var texture = new GradientTexture2D
        {
            Gradient = gradient,
            Fill     = GradientTexture2D.FillEnum.Radial,
            FillFrom = new Vector2(0.5f, 0.5f),
            FillTo   = new Vector2(1.0f, 0.5f),
            Width    = 256,
            Height   = 256,
        };

        _vignette = new TextureRect
        {
            Texture     = texture,
            ExpandMode  = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            MouseFilter = MouseFilterEnum.Ignore,
            Modulate    = new Color(1, 1, 1, 0f),
        };
        _vignette.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(_vignette);

        GameManager.Instance.StateChanged += OnStateChanged;
        OnStateChanged();
    }

    public override void _ExitTree()
    {
        GameManager.Instance.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged()
    {
        var gm = GameManager.Instance;
        var p  = gm.Player;

        _active = p != null
               && gm.Turns.State != TurnState.GameOver
               && p.Stats.MaxHp > 0
               && (float)p.Stats.Hp / p.Stats.MaxHp < Threshold;

        SetProcess(_active);
        if (!_active)
            _vignette.Modulate = new Color(1, 1, 1, 0f);
    }

    public override void _Process(double delta)
    {
        if (!_active) return;

        _time += delta;
        // Slow, gentle breath; alpha oscillates between 0.15 and 0.55
        float pulse = 0.15f + 0.40f * (0.5f + 0.5f * MathF.Sin((float)_time * 3.2f));
        _vignette.Modulate = new Color(1, 1, 1, pulse);
    }
}
