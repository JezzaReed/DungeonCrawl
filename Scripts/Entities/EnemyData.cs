using System;
using Godot;

namespace NewGameProject;

public class EnemyData
{
    public EnemyType   Type    { get; }
    public EntityStats Stats   { get; }
    public Vector2I    GridPos { get; set; }
    public bool        IsBoss  { get; }
    public bool        IsDead  => !Stats.IsAlive;

    public EnemyData(EnemyType type, Vector2I pos, int floor, bool isBoss = false)
    {
        Type    = type;
        GridPos = pos;
        IsBoss  = isBoss;
        Stats   = MakeStats(type, floor);

        if (isBoss)
        {
            Stats.MaxHp    = Stats.MaxHp * 3;
            Stats.Hp       = Stats.MaxHp;
            Stats.Attack  += floor / 2 + 2;
            Stats.Defense += 2;
            Stats.XpReward = Stats.XpReward * 5;
            Stats.Name     = BossNameFor(floor);
        }
    }

    // ── Boss identity ──────────────────────────────────────────────
    public static bool IsBossFloor(int floor) => floor % 5 == 0;

    /// <summary>Which (visual) enemy type represents the boss on this floor.</summary>
    public static EnemyType BossTypeFor(int floor) => (floor / 5) switch
    {
        1   => EnemyType.Orc,
        2   => EnemyType.Troll,
        _   => EnemyType.Dragon,
    };

    private static string BossNameFor(int floor) => (floor / 5) switch
    {
        1   => "Gruul the Orc Warlord",
        2   => "Morggok the Troll King",
        3   => "Vaskarr the Dragon Tyrant",
        _   => "Ancient Wyrm of the Deep",
    };

    private static EntityStats MakeStats(EnemyType t, int floor) => t switch
    {
        EnemyType.Goblin   => new EntityStats { Name = "Goblin",   MaxHp = 8  + floor * 2, Hp = 8  + floor * 2, Attack = 3 + floor,     Defense = 0, XpReward = 15  },
        EnemyType.Skeleton => new EntityStats { Name = "Skeleton", MaxHp = 12 + floor * 2, Hp = 12 + floor * 2, Attack = 4 + floor,     Defense = 1, XpReward = 25  },
        EnemyType.Orc      => new EntityStats { Name = "Orc",      MaxHp = 20 + floor * 3, Hp = 20 + floor * 3, Attack = 6 + floor,     Defense = 2, XpReward = 40  },
        EnemyType.Troll    => new EntityStats { Name = "Troll",    MaxHp = 35 + floor * 5, Hp = 35 + floor * 5, Attack = 9 + floor * 2, Defense = 3, XpReward = 80  },
        EnemyType.Dragon   => new EntityStats { Name = "Dragon",   MaxHp = 80 + floor * 10,Hp = 80 + floor * 10,Attack = 15+ floor * 3, Defense = 5, XpReward = 250 },
        _                  => new EntityStats { Name = "Unknown",  MaxHp = 10,              Hp = 10,              Attack = 3,             Defense = 0, XpReward = 5   },
    };

    // One-step greedy move toward player; returns current pos if blocked
    public Vector2I GetNextMove(Vector2I playerPos, DungeonData dungeon)
    {
        int dx = playerPos.X - GridPos.X;
        int dy = playerPos.Y - GridPos.Y;
        int sx = dx == 0 ? 0 : Math.Sign(dx);
        int sy = dy == 0 ? 0 : Math.Sign(dy);

        var tryH = new Vector2I(GridPos.X + sx, GridPos.Y);
        var tryV = new Vector2I(GridPos.X,      GridPos.Y + sy);

        if (Math.Abs(dx) >= Math.Abs(dy))
        {
            if (sx != 0 && dungeon.IsWalkable(tryH)) return tryH;
            if (sy != 0 && dungeon.IsWalkable(tryV)) return tryV;
        }
        else
        {
            if (sy != 0 && dungeon.IsWalkable(tryV)) return tryV;
            if (sx != 0 && dungeon.IsWalkable(tryH)) return tryH;
        }
        return GridPos;
    }
}
