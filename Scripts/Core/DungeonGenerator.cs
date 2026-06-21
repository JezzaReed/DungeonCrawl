using System;
using System.Collections.Generic;
using Godot;

namespace NewGameProject;

public class DungeonGenerator
{
    private Random _rng = new();
    private DungeonData _d = null!;

    public DungeonData Generate(int width, int height, int floor)
    {
        _rng = new Random();
        _d   = new DungeonData(width, height);

        // Fill with walls
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _d.Tiles[x, y] = TileType.Wall;

        int targetRooms = Math.Clamp(6 + floor * 2, 6, 20);

        for (int attempt = 0; attempt < 300 && _d.Rooms.Count < targetRooms; attempt++)
        {
            int w = _rng.Next(5, 14);
            int h = _rng.Next(4, 10);
            int x = _rng.Next(1, width  - w - 1);
            int y = _rng.Next(1, height - h - 1);
            var room = new Room(x, y, w, h);

            bool ok = true;
            foreach (var existing in _d.Rooms)
                if (room.Intersects(existing)) { ok = false; break; }
            if (!ok) continue;

            CarveRoom(room);
            if (_d.Rooms.Count > 0)
                ConnectRooms(_d.Rooms[^1].Center, room.Center);
            _d.Rooms.Add(room);
        }

        _d.PlayerStart = _d.Rooms[0].Center;

        var lastRoom = _d.Rooms[^1];
        _d.StairsPos = lastRoom.Center;
        _d.Tiles[lastRoom.Center.X, lastRoom.Center.Y] = TileType.StairsDown;

        SpawnEntities(floor);
        return _d;
    }

    private void CarveRoom(Room r)
    {
        for (int x = r.X; x < r.X + r.Width; x++)
            for (int y = r.Y; y < r.Y + r.Height; y++)
                _d.Tiles[x, y] = TileType.Floor;
    }

    private void ConnectRooms(Vector2I a, Vector2I b)
    {
        if (_rng.Next(2) == 0) { CarveH(a.X, b.X, a.Y); CarveV(a.Y, b.Y, b.X); }
        else                   { CarveV(a.Y, b.Y, a.X); CarveH(a.X, b.X, b.Y); }
    }

    private void CarveH(int x1, int x2, int y)
    {
        for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
            if (_d.InBounds(x, y)) _d.Tiles[x, y] = TileType.Floor;
    }

    private void CarveV(int y1, int y2, int x)
    {
        for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
            if (_d.InBounds(x, y)) _d.Tiles[x, y] = TileType.Floor;
    }

    private void SpawnEntities(int floor)
    {
        var allEnemies = (EnemyType[])Enum.GetValues(typeof(EnemyType));
        int maxIdx = Math.Clamp(floor, 0, allEnemies.Length - 1);
        var allItems = (ItemType[])Enum.GetValues(typeof(ItemType));

        bool bossFloor = EnemyData.IsBossFloor(floor);
        int  bossRoom  = _d.Rooms.Count - 1; // last room = stairs room = arena

        for (int i = 1; i < _d.Rooms.Count; i++)
        {
            var room = _d.Rooms[i];

            // The boss room is reserved for the boss + its hoard
            if (bossFloor && i == bossRoom) continue;

            int count = _rng.Next(1, 2 + floor / 2);
            for (int e = 0; e < count; e++)
                _d.EnemySpawns.Add((RandomInRoom(room), allEnemies[_rng.Next(0, maxIdx + 1)]));

            if (_rng.Next(3) == 0)
                _d.ItemSpawns.Add((RandomInRoom(room), allItems[_rng.Next(allItems.Length)]));
        }

        if (bossFloor && _d.Rooms.Count > 1)
        {
            var arena = _d.Rooms[bossRoom];

            // Boss guards the stairs; place it a little off the exact stairs tile
            Vector2I bossPos = RandomInRoom(arena);
            if (bossPos == _d.StairsPos) bossPos += new Vector2I(1, 0);
            _d.BossSpawn = (bossPos, EnemyData.BossTypeFor(floor));

            // Guaranteed hoard as a reward for clearing the floor
            _d.ItemSpawns.Add((RandomInRoom(arena), ItemType.HealthPotion));
            _d.ItemSpawns.Add((RandomInRoom(arena), allItems[_rng.Next(allItems.Length)]));
        }
    }

    private Vector2I RandomInRoom(Room r) => new(
        _rng.Next(r.X + 1, r.X + r.Width  - 1),
        _rng.Next(r.Y + 1, r.Y + r.Height - 1));
}
