using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace NewGameProject;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; } = null!;

    // ── Game state ──────────────────────────────────────────────────
    public PlayerData?          Player        { get; private set; }
    public DungeonData?         Dungeon       { get; private set; }
    public List<EnemyData>      Enemies       { get; private set; } = new();
    public List<Item>           Items         { get; private set; } = new();
    public MessageLog           Log           { get; private set; } = new();
    public TurnManager          Turns         { get; private set; } = new();
    public HashSet<Vector2I>    VisibleTiles  { get; private set; } = new();
    public HashSet<Vector2I>    ExploredTiles { get; private set; } = new();

    [Signal] public delegate void StateChangedEventHandler();

    // ── Last finished run (for the game-over screen / leaderboard) ──
    public RunRecord? LastRun     { get; private set; }
    public int        LastRunRank { get; private set; } = -1;

    // ── Pending between-floor upgrade choice ───────────────────────
    public IReadOnlyList<ItemType>? PendingUpgrades { get; private set; }

    // ── Pause ──────────────────────────────────────────────────────
    public bool IsPaused { get; private set; }

    private readonly DungeonGenerator _gen = new();
    private readonly Random           _rng = new();

    public override void _Ready() => Instance = this;

    // ── New game ────────────────────────────────────────────────────
    public void StartNewGame()
    {
        Player        = new PlayerData();
        Log           = new MessageLog();
        Turns         = new TurnManager();
        ExploredTiles = new HashSet<Vector2I>();
        GenerateFloor(1);
        Log.Add("Welcome to the dungeon! Arrow keys / WASD to move. Bump enemies to attack.", "#888888");
        Log.Add("Step onto [>] to descend. Items are picked up automatically.", "#888888");
        EmitSignal(SignalName.StateChanged);
    }

    // ── Floor generation ────────────────────────────────────────────
    public void GenerateFloor(int floor)
    {
        Dungeon = _gen.Generate(100, 70, floor);
        Enemies = new List<EnemyData>();
        Items   = new List<Item>();

        foreach (var (pos, type) in Dungeon.EnemySpawns)
            Enemies.Add(new EnemyData(type, pos, floor));
        foreach (var (pos, type) in Dungeon.ItemSpawns)
            Items.Add(new Item(type, pos));

        if (Dungeon.BossSpawn is { } bs)
        {
            var boss = new EnemyData(bs.type, bs.pos, floor, isBoss: true);
            Enemies.Add(boss);
            Log.Add($"⚔ {boss.Stats.Name} guards the stairs on this floor!", "#ff5555");
        }

        Player!.GridPos = Dungeon.PlayerStart;
        Player!.Floor   = floor;
        UpdateFov();
    }

    public void UpdateFov()
    {
        if (Player == null || Dungeon == null) return;
        VisibleTiles = FovSystem.ComputeFov(Dungeon, Player.GridPos, 12);
        foreach (var t in VisibleTiles) ExploredTiles.Add(t);
    }

    // ── Player actions ──────────────────────────────────────────────
    public bool TryMovePlayer(Vector2I delta)
    {
        if (Player == null || Dungeon == null) return false;
        if (Turns.State != TurnState.PlayerTurn) return false;
        if (PendingUpgrades != null || IsPaused) return false;

        var newPos = Player.GridPos + delta;

        var enemy = GetEnemyAt(newPos);
        if (enemy != null)
        {
            MeleeAttack(Player.Stats, enemy.Stats, $"You", enemy.Stats.Name);
            if (enemy.IsDead)
            {
                if (enemy.IsBoss)
                {
                    Log.Add($"★ {enemy.Stats.Name} has been vanquished! The way down is clear.", "#ffdd44");
                    Player.Score += enemy.Stats.XpReward; // extra boss bonus on top of the kill reward
                }
                else
                {
                    Log.Add($"The {enemy.Stats.Name} is slain!", "#ff8844");
                }
                Player.AddXp(enemy.Stats.XpReward, Log);
                Player.KillCount++;
                Player.Score += enemy.Stats.XpReward;
            }
            EndPlayerTurn();
            return true;
        }

        if (!Dungeon.IsWalkable(newPos)) return false;

        Player.GridPos = newPos;
        UpdateFov();

        var item = GetItemAt(newPos);
        if (item != null && !item.Collected && item.Apply(Player, Log))
        {
            item.Collected = true;
            Player.Score += 10;
        }

        if (Dungeon.Tiles[newPos.X, newPos.Y] == TileType.StairsDown)
            DescendStairs();
        else
            EndPlayerTurn();

        return true;
    }

    public bool TryWait()
    {
        if (Turns.State != TurnState.PlayerTurn) return false;
        if (PendingUpgrades != null || IsPaused) return false;
        Log.Add("You wait.", "#666666");
        EndPlayerTurn();
        return true;
    }

    public bool TryUsePotion()
    {
        if (Player == null) return false;
        if (Turns.State != TurnState.PlayerTurn) return false;
        if (PendingUpgrades != null || IsPaused) return false;

        if (Player.Potions <= 0)
        {
            Log.Add("You have no potions.", "#888888");
            EmitSignal(SignalName.StateChanged);
            return false;
        }
        if (Player.Stats.Hp >= Player.Stats.MaxHp)
        {
            Log.Add("You are already at full health.", "#888888");
            EmitSignal(SignalName.StateChanged);
            return false;
        }

        int heal = Player.PotionHeal;
        Player.Potions--;
        Player.Stats.Heal(heal);
        Log.Add($"You drink a Health Potion, restoring {heal} HP. ({Player.Potions} left)", "#ff6666");
        EndPlayerTurn();
        return true;
    }

    // ── Pause / resign ──────────────────────────────────────────────
    public void TogglePause()
    {
        if (Player == null || Turns.State == TurnState.GameOver || PendingUpgrades != null) return;
        IsPaused = !IsPaused;
        EmitSignal(SignalName.StateChanged);
    }

    public void ResumeGame()
    {
        if (!IsPaused) return;
        IsPaused = false;
        EmitSignal(SignalName.StateChanged);
    }

    /// <summary>Give up the run — banks the current score on the leaderboard, as if killed.</summary>
    public void Resign()
    {
        if (Player == null) return;
        IsPaused = false;
        Player.KilledBy = "Resignation";
        Turns.SetGameOver();
        RecordRun();
        EmitSignal(SignalName.StateChanged);
    }

    // ── Internal helpers ────────────────────────────────────────────
    private void MeleeAttack(EntityStats attacker, EntityStats defender, string attackerName, string defenderName)
    {
        int dmg = defender.TakeDamage(attacker.Attack);
        bool isPlayer = attackerName == "You";
        string color = isPlayer ? "#ffcc44" : "#ff4444";
        Log.Add($"{attackerName} hit{(isPlayer ? "" : "s")} the {defenderName} for {dmg} damage!", color);
    }

    private void EndPlayerTurn()
    {
        Turns.EndPlayerTurn();
        ProcessEnemyTurns();
        EmitSignal(SignalName.StateChanged);
    }

    private void ProcessEnemyTurns()
    {
        if (Player == null || Dungeon == null) return;

        foreach (var enemy in Enemies)
        {
            if (enemy.IsDead) continue;

            // Enemies only act when visible
            if (!VisibleTiles.Contains(enemy.GridPos)) continue;

            int dx = Math.Abs(enemy.GridPos.X - Player.GridPos.X);
            int dy = Math.Abs(enemy.GridPos.Y - Player.GridPos.Y);

            if (dx <= 1 && dy <= 1 && dx + dy > 0)
            {
                // Adjacent — attack
                MeleeAttack(enemy.Stats, Player.Stats, $"The {enemy.Stats.Name}", "you");
                if (!Player.Stats.IsAlive)
                {
                    Player.KilledBy = enemy.Stats.Name;
                    Log.Add("You have died. GAME OVER.", "#ff2222");
                    Turns.SetGameOver();
                    RecordRun();
                    EmitSignal(SignalName.StateChanged);
                    return;
                }
            }
            else
            {
                var next = enemy.GetNextMove(Player.GridPos, Dungeon);
                if (next != enemy.GridPos && GetEnemyAt(next) == null && next != Player.GridPos)
                    enemy.GridPos = next;
            }
        }

        Enemies.RemoveAll(e => e.IsDead);
        Turns.EndEnemyTurn();
    }

    private void RecordRun()
    {
        if (Player == null) return;

        LastRun = new RunRecord
        {
            Score    = Player.Score,
            Floor    = Player.Floor,
            Level    = Player.Level,
            Kills    = Player.KillCount,
            KilledBy = Player.KilledBy,
            Date     = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
        };
        LastRunRank = Leaderboard.Submit(LastRun);
    }

    private void DescendStairs()
    {
        int next = Player!.Floor + 1;
        Log.Add($"You descend to floor {next}. The darkness deepens…", "#888888");
        ExploredTiles.Clear();
        GenerateFloor(next);
        OfferUpgrades();
        EmitSignal(SignalName.StateChanged);
    }

    // ── Between-floor upgrade choice ────────────────────────────────
    private void OfferUpgrades()
    {
        var all = (ItemType[])Enum.GetValues(typeof(ItemType));

        // Pick two distinct options
        int a = _rng.Next(all.Length);
        int b = _rng.Next(all.Length - 1);
        if (b >= a) b++;

        PendingUpgrades = new[] { all[a], all[b] };
        Log.Add("Choose a reward for reaching the next floor.", "#c8a020");
    }

    public void ChooseUpgrade(ItemType type)
    {
        if (Player == null || PendingUpgrades == null) return;
        if (!PendingUpgrades.Contains(type)) return;

        new Item(type, Vector2I.Zero).Apply(Player, Log);
        PendingUpgrades = null;
        EmitSignal(SignalName.StateChanged);
    }

    // ── Queries ─────────────────────────────────────────────────────
    public EnemyData? GetEnemyAt(Vector2I pos) => Enemies.Find(e => e.GridPos == pos && !e.IsDead);
    public Item?      GetItemAt(Vector2I pos)  => Items.Find(i => i.GridPos == pos && !i.Collected);
}
