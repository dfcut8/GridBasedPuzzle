using System.Collections.Generic;

using Godot;

namespace GridBasedPuzzle.Scenes.Core;

public static class GodotExtensions
{
    public static List<Vector2I> GetTiles(this Rect2I r)
    {
        List<Vector2I> result = [];
        for (int x = r.Position.X; x < r.End.X; x++)
        {
            for (int y = r.Position.Y; y < r.End.Y; y++)
            {
                result.Add(new Vector2I(x, y));
            }
        }
        return result;
    }

    public static Rect2 ToRect(this Rect2I r)
    {
        return new Rect2(r.Position, r.Size);
    }
}
