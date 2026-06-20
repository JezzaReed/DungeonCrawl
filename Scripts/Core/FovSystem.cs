using System;
using System.Collections.Generic;
using Godot;

namespace NewGameProject;

public static class FovSystem
{
    public static HashSet<Vector2I> ComputeFov(DungeonData dungeon, Vector2I origin, int radius)
    {
        var visible = new HashSet<Vector2I> { origin };

        for (int angle = 0; angle < 360; angle++)
        {
            float rad = angle * MathF.PI / 180f;
            float dx  = MathF.Cos(rad);
            float dy  = MathF.Sin(rad);
            float x   = origin.X + 0.5f;
            float y   = origin.Y + 0.5f;

            for (int r = 0; r < radius; r++)
            {
                x += dx;
                y += dy;
                var pos = new Vector2I((int)x, (int)y);
                if (!dungeon.InBounds(pos)) break;
                visible.Add(pos);
                if (dungeon.Tiles[pos.X, pos.Y] == TileType.Wall) break;
            }
        }

        return visible;
    }
}
