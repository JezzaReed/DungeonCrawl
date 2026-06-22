using Godot;

namespace NewGameProject;

/// <summary>
/// Full-screen overlay shown after descending: pick 1 of 2 random upgrades.
/// Hidden whenever <see cref="GameManager.PendingUpgrades"/> is null.
/// </summary>
public partial class UpgradeChoiceUI : Control
{
    private HBoxContainer _options = null!;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop; // swallow clicks while visible

        // Dimmed backdrop
        var bg = new ColorRect { Color = new Color(0, 0, 0, 0.72f), LayoutMode = 1 };
        bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        var vbox = new VBoxContainer { LayoutMode = 1 };
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
        vbox.CustomMinimumSize = new Vector2(540, 320);
        vbox.Position -= vbox.CustomMinimumSize / 2f;
        vbox.AddThemeConstantOverride("separation", 20);
        AddChild(vbox);

        var title = new Label { Text = "CHOOSE YOUR REWARD" };
        title.AddThemeFontSizeOverride("font_size", 34);
        title.AddThemeColorOverride("font_color", Color.FromHtml("#c8a020"));
        title.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(title);

        _options = new HBoxContainer();
        _options.AddThemeConstantOverride("separation", 28);
        _options.Alignment = BoxContainer.AlignmentMode.Center;
        vbox.AddChild(_options);

        var hint = new Label { Text = "Click a card, or press [1] / [2]" };
        hint.AddThemeFontSizeOverride("font_size", 14);
        hint.AddThemeColorOverride("font_color", Color.FromHtml("#776655"));
        hint.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(hint);

        GameManager.Instance.StateChanged += Refresh;
        Refresh();
    }

    public override void _ExitTree()
    {
        GameManager.Instance.StateChanged -= Refresh;
    }

    public override void _Input(InputEvent ev)
    {
        if (!Visible) return;
        if (ev is not InputEventKey key || !key.Pressed || key.Echo) return;

        var pending = GameManager.Instance.PendingUpgrades;
        if (pending == null) return;

        int pick = key.Keycode switch
        {
            Key.Key1 or Key.Kp1 => 0,
            Key.Key2 or Key.Kp2 => 1,
            _                   => -1,
        };
        if (pick >= 0 && pick < pending.Count)
        {
            GameManager.Instance.ChooseUpgrade(pending[pick]);
            GetViewport().SetInputAsHandled();
        }
    }

    private void Refresh()
    {
        var pending = GameManager.Instance.PendingUpgrades;
        Visible = pending != null;
        if (pending == null) return;

        foreach (var c in _options.GetChildren()) c.QueueFree();

        for (int i = 0; i < pending.Count; i++)
            _options.AddChild(MakeCard(pending[i], i + 1));
    }

    private Control MakeCard(ItemType type, int index)
    {
        var card = new VBoxContainer();
        card.AddThemeConstantOverride("separation", 8);
        card.CustomMinimumSize = new Vector2(220, 0);

        var icon = new TextureRect
        {
            Texture           = GD.Load<Texture2D>($"res://Textures/{TextureName(type)}.svg"),
            StretchMode       = TextureRect.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = new Vector2(0, 72),
        };
        card.AddChild(icon);

        var name = new Label { Text = Item.NameOf(type), HorizontalAlignment = HorizontalAlignment.Center };
        name.AddThemeFontSizeOverride("font_size", 20);
        name.AddThemeColorOverride("font_color", Color.FromHtml("#ffffff"));
        card.AddChild(name);

        var effect = new Label { Text = EffectText(type), HorizontalAlignment = HorizontalAlignment.Center };
        effect.AddThemeFontSizeOverride("font_size", 15);
        effect.AddThemeColorOverride("font_color", Color.FromHtml("#aa8844"));
        card.AddChild(effect);

        var btn = new Button { Text = $"Take [{index}]", CustomMinimumSize = new Vector2(200, 44) };
        btn.AddThemeFontSizeOverride("font_size", 18);
        btn.Pressed += () => GameManager.Instance.ChooseUpgrade(type);
        card.AddChild(btn);

        return card;
    }

    private static string TextureName(ItemType t) => t switch
    {
        ItemType.HealthPotion => "health_potion",
        ItemType.Sword        => "sword",
        ItemType.Shield       => "shield",
        ItemType.Scroll       => "scroll",
        _                     => "health_potion",
    };

    private static string EffectText(ItemType t) => t switch
    {
        ItemType.HealthPotion => "Store a potion ([Q] to drink)",
        ItemType.Sword        => "Attack +3",
        ItemType.Shield       => "Defense +2",
        ItemType.Scroll       => "Attack +5",
        _                     => "",
    };
}
