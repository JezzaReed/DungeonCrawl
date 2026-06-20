using System.Collections.Generic;
using Godot;

namespace NewGameProject;

public class Room
{
    public int X, Y, Width, Height;

    public Room(int x, int y, int w, int h) { X = x; Y = y; Width = w; Height = h; }

    public Vector2I Center => new(X + Width / 2, Y + Height / 2);

    public bool Intersects(Room o, int pad = 2) =>
        X - pad < o.X + o.Width  && X + Width  + pad > o.X &&
        Y - pad < o.Y + o.Height && Y + Height + pad > o.Y;
}

public class DungeonData
{
    public int Width  { get; }
    public int Height { get; }
    public TileType[,] Tiles { get; }
    public List<Room> Rooms { get; } = new();
    public Vector2I PlayerStart { get; set; }
    public Vector2I StairsPos   { get; set; }
    public List<(Vector2I pos, EnemyType type)> EnemySpawns { get; } = new();
    public List<(Vector2I pos, ItemType  type)> ItemSpawns  { get; } = new();

    public DungeonData(int w, int h)
    {
        Width  = w;
        Height = h;
        Tiles  = new TileType[w, h];
    }

    public bool InBounds(int x, int y)  => x >= 0 && x < Width && y >= 0 && y < Height;
    public bool InBounds(Vector2I p)    => InBounds(p.X, p.Y);
    public bool IsWalkable(Vector2I p)  => InBounds(p) &&
                                           Tiles[p.X, p.Y] != TileType.Wall &&
                                           Tiles[p.X, p.Y] != TileType.None;
}
