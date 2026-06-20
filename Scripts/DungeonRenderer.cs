using System;
using System.Collections.Generic;
using Godot;

namespace NewGameProject;

public partial class DungeonRenderer : Node2D
{
    private const int TS        = 16;
    private const int HudHeight = 144;

    // ── Hold-to-move ─────────────────────────────────────────────────
    private Vector2I _heldDir     = Vector2I.Zero;
    private double   _holdTimer   = 0.0;
    private double   _repeatDelay = 0.0;
    private const double InitialDelay  = 0.20;
    private const double StartInterval = 0.12;
    private const double MinInterval   = 0.04;
    private const double SpeedUp       = 0.010;

    // ── Textures ─────────────────────────────────────────────────────
    private Texture2D _texFloor  = null!;
    private Texture2D _texWall   = null!;
    private Texture2D _texStairs = null!;
    private Texture2D _texPlayer = null!;
    private readonly Dictionary<EnemyType, Texture2D> _texEnemies = new();
    private readonly Dictionary<ItemType,  Texture2D> _texItems   = new();

    private Camera2D _camera = null!;

    // ── Lifecycle ─────────────────────────────────────────────────────
    public override void _Ready()
    {
        LoadTextures();

        _camera = new Camera2D { Offset = new Vector2(0, HudHeight / 2f) };
        AddChild(_camera);

        var gm = GameManager.Instance;
        gm.StateChanged += OnStateChanged;
        gm.StartNewGame();
        SyncCamera();
        QueueRedraw();
    }

    public override void _ExitTree()
    {
        GameManager.Instance.StateChanged -= OnStateChanged;
    }

    private void LoadTextures()
    {
        _texFloor  = Load("floor");
        _texWall   = Load("wall");
        _texStairs = Load("stairs_down");
        _texPlayer = Load("hero");

        _texEnemies[EnemyType.Goblin]   = Load("goblin");
        _texEnemies[EnemyType.Skeleton] = Load("skeleton");
        _texEnemies[EnemyType.Orc]      = Load("orc");
        _texEnemies[EnemyType.Troll]    = Load("troll");
        _texEnemies[EnemyType.Dragon]   = Load("dragon");

        _texItems[ItemType.HealthPotion] = Load("health_potion");
        _texItems[ItemType.Sword]        = Load("sword");
        _texItems[ItemType.Shield]       = Load("shield");
        _texItems[ItemType.Scroll]       = Load("scroll");
    }

    private static Texture2D Load(string name) =>
        GD.Load<Texture2D>($"res://Textures/{name}.svg");

    // ── Input ─────────────────────────────────────────────────────────
    public override void _Input(InputEvent ev)
    {
        if (ev is not InputEventKey key || key.Echo) return;

        var gm = GameManager.Instance;

        if (gm.Turns.State == TurnState.GameOver)
        {
            if (key.Pressed) GetTree().ChangeSceneToFile("res://Scenes/GameOver.tscn");
            return;
        }

        if (!key.Pressed)
        {
            if (_heldDir != Vector2I.Zero && KeyToDir(key.Keycode) == _heldDir)
                _heldDir = Vector2I.Zero;
            return;
        }

        var dir = KeyToDir(key.Keycode);
        if (dir != Vector2I.Zero)
        {
            gm.TryMovePlayer(dir);
            _heldDir     = dir;
            _holdTimer   = InitialDelay;
            _repeatDelay = StartInterval;
            return;
        }

        switch (key.Keycode)
        {
            case Key.Period or Key.Kp5: gm.TryWait(); break;
            case Key.Escape:            ReturnToMenu(); break;
        }
    }

    // ── Hold repeat ───────────────────────────────────────────────────
    public override void _Process(double delta)
    {
        if (_heldDir == Vector2I.Zero) return;

        var gm = GameManager.Instance;
        if (gm.Turns.State != TurnState.PlayerTurn)
        {
            _heldDir = Vector2I.Zero;
            return;
        }

        _holdTimer -= delta;
        if (_holdTimer <= 0.0)
        {
            gm.TryMovePlayer(_heldDir);
            _repeatDelay = Math.Max(MinInterval, _repeatDelay - SpeedUp);
            _holdTimer   = _repeatDelay;
        }
    }

    private bool ReturnToMenu()
    {
        _heldDir = Vector2I.Zero;
        GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
        return true;
    }

    private void OnStateChanged()
    {
        SyncCamera();
        QueueRedraw();
    }

    private void SyncCamera()
    {
        var gm = GameManager.Instance;
        if (gm.Player == null || gm.Dungeon == null) return;

        _camera.Position = new Vector2(
            gm.Player.GridPos.X * TS + TS / 2f,
            gm.Player.GridPos.Y * TS + TS / 2f);

        _camera.LimitLeft   = 0;
        _camera.LimitTop    = 0;
        _camera.LimitRight  = gm.Dungeon.Width  * TS;
        _camera.LimitBottom = gm.Dungeon.Height * TS;
    }

    // ── Draw ─────────────────────────────────────────────────────────
    public override void _Draw()
    {
        var gm = GameManager.Instance;
        if (gm.Dungeon == null || gm.Player == null) return;

        var dungeon  = gm.Dungeon;
        var visible  = gm.VisibleTiles;
        var explored = gm.ExploredTiles;

        for (int x = 0; x < dungeon.Width;  x++)
        for (int y = 0; y < dungeon.Height; y++)
        {
            var pos  = new Vector2I(x, y);
            var tile = dungeon.Tiles[x, y];
            var rect = new Rect2(x * TS, y * TS, TS, TS);

            if (!visible.Contains(pos) && !explored.Contains(pos)) continue;

            // Terrain
            var terrainTex = tile switch
            {
                TileType.Wall       => _texWall,
                TileType.StairsDown => _texStairs,
                _                   => _texFloor,
            };
            DrawTextureRect(terrainTex, rect, false);

            // Item
            var item = gm.GetItemAt(pos);
            if (item != null && !item.Collected && _texItems.TryGetValue(item.Type, out var itemTex))
                DrawTextureRect(itemTex, rect, false);

            // Enemy
            var enemy = gm.GetEnemyAt(pos);
            if (enemy != null && _texEnemies.TryGetValue(enemy.Type, out var enemyTex))
                DrawTextureRect(enemyTex, rect, false);
        }

        // Player always on top
        var prect = new Rect2(gm.Player.GridPos.X * TS, gm.Player.GridPos.Y * TS, TS, TS);
        DrawTextureRect(_texPlayer, prect, false);
    }

    // ── Helpers ───────────────────────────────────────────────────────
    private static Vector2I KeyToDir(Key k) => k switch
    {
        Key.Up    or Key.W or Key.Kp8 => new Vector2I( 0, -1),
        Key.Down  or Key.S or Key.Kp2 => new Vector2I( 0,  1),
        Key.Left  or Key.A or Key.Kp4 => new Vector2I(-1,  0),
        Key.Right or Key.D or Key.Kp6 => new Vector2I( 1,  0),
        Key.Y     or Key.Kp7           => new Vector2I(-1, -1),
        Key.U     or Key.Kp9           => new Vector2I( 1, -1),
        Key.B     or Key.Kp1           => new Vector2I(-1,  1),
        Key.N     or Key.Kp3           => new Vector2I( 1,  1),
        _                              => Vector2I.Zero,
    };
}
