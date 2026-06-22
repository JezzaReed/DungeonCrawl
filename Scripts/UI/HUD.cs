using Godot;
using System;
using System.Text;

namespace NewGameProject;

/// <summary>
/// Single bottom panel — sits below the 80×36 dungeon (576 px).
/// Left side: player stats. Right side: scrolling message log.
/// No overlap with the dungeon view.
/// </summary>
public partial class HUD : Control
{
    internal const int PanelHeight = 144; // 720 - 36*16
    private const int StatsWidth  = 460;
    private const int MsgWidth    = 560;
    private const int MsgLines    = 6;

    private RichTextLabel _statsLabel = null!;
    private RichTextLabel _msgLabel   = null!;

    public override void _Ready()
    {
        // ── Root panel anchored to bottom of screen ──────────────────
        var panel = new PanelContainer();
        panel.AnchorLeft   = 0f;
        panel.AnchorRight  = 1f;
        panel.AnchorTop    = 1f;
        panel.AnchorBottom = 1f;
        panel.OffsetTop    = -PanelHeight;
        panel.OffsetBottom = 0f;
        panel.OffsetLeft   = 0f;
        panel.OffsetRight  = 0f;
        ApplyPanelStyle(panel);
        AddChild(panel);

        // ── Horizontal split ─────────────────────────────────────────
        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 12);
        panel.AddChild(hbox);

        // Left: stats
        var statsBox = new VBoxContainer();
        statsBox.CustomMinimumSize = new Vector2(StatsWidth, 0);
        statsBox.AddThemeConstantOverride("separation", 4);
        hbox.AddChild(statsBox);

        _statsLabel = new RichTextLabel
        {
            BbcodeEnabled  = true,
            FitContent     = false,
            ScrollActive   = false,
            AutowrapMode   = TextServer.AutowrapMode.Off, // keep to 4 fixed lines
            CustomMinimumSize = new Vector2(StatsWidth, PanelHeight - 16),
        };
        statsBox.AddChild(_statsLabel);

        // Divider
        var div = new VSeparator();
        hbox.AddChild(div);

        // Right: message log
        _msgLabel = new RichTextLabel
        {
            BbcodeEnabled    = true,
            ScrollFollowing  = true,
            ScrollActive     = false,
            CustomMinimumSize   = new Vector2(MsgWidth, PanelHeight - 16),
        };
        hbox.AddChild(_msgLabel);

        // Trailing spacer absorbs surplus width so the message panel stays a fixed size
        hbox.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });

        GameManager.Instance.StateChanged += Refresh;
        Refresh();
    }

    public override void _ExitTree()
    {
        GameManager.Instance.StateChanged -= Refresh;
    }

    private void Refresh()
    {
        var gm = GameManager.Instance;
        if (gm.Player == null) return;
        var s = gm.Player.Stats;

        float hpFrac  = (float)s.Hp / s.MaxHp;
        string hpCol  = hpFrac > 0.5f ? "#66ee66" : hpFrac > 0.25f ? "#ffcc44" : "#ff4444";
        string hpBar  = BuildBar(hpFrac, 12, hpCol);

        float xpFrac  = (float)gm.Player.Xp / gm.Player.XpToNextLevel;
        string xpBar  = BuildBar(xpFrac, 12, "#8866ff");

        _statsLabel.Text =
            $"[b][color=#c8a020]DUNGEON CRAWL[/color][/b]   " +
            $"Floor [color=#ffffff]{gm.Player.Floor}[/color]   " +
            $"[color=#aaaacc]LVL[/color] [color=#ffffff]{gm.Player.Level}[/color]\n" +
            $"[color=#aaaacc]HP[/color]  {hpBar} [color={hpCol}]{s.Hp}[/color][color=#555555]/{s.MaxHp}[/color]\n" +
            $"[color=#aaaacc]XP[/color]  {xpBar} [color=#8866ff]{gm.Player.Xp}[/color][color=#555555]/{gm.Player.XpToNextLevel}[/color]\n" +
            $"[color=#aaaacc]ATK[/color] {s.Attack}  [color=#aaaacc]DEF[/color] {s.Defense}  " +
            $"[color=#ff6666]Pot[/color] {gm.Player.Potions}[color=#555555]/{PlayerData.MaxPotions}[/color]  " +
            $"[color=#aaaacc]Kills[/color] {gm.Player.KillCount}  [color=#aaaacc]Score[/color] {gm.Player.Score}";

        var msgs = gm.Log.Messages;
        int start = Math.Max(0, msgs.Count - MsgLines);
        var sb = new StringBuilder();
        for (int i = start; i < msgs.Count; i++)
            sb.AppendLine($"[color={msgs[i].Color}]{msgs[i].Text}[/color]");
        _msgLabel.Text = sb.ToString();
    }

    private static string BuildBar(float frac, int len, string color)
    {
        int filled = (int)(frac * len);
        string bar = new string('█', filled) + new string('░', len - filled);
        return $"[color={color}]{bar}[/color]";
    }

    private static void ApplyPanelStyle(PanelContainer panel)
    {
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor             = new Color(0.06f, 0.05f, 0.04f, 0.96f),
            BorderWidthTop      = 1,
            BorderColor         = new Color(0.35f, 0.28f, 0.20f, 1f),
            ContentMarginLeft   = 10,
            ContentMarginRight  = 10,
            ContentMarginTop    = 8,
            ContentMarginBottom = 8,
        });
    }
}
