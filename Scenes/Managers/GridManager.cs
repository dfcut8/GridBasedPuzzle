using Godot;

namespace GridBasedPuzzle.Managers;

public partial class GridManager : Node
{
    [Export] private TileMapLayer highLightTilemapLayer;
    [Export] private TileMapLayer baseTerrainTilemapLayer;

    public void HighlightValidTilesInRadius(Vector2 rootCell, int radius)
    {
        ClearHighlightedTiles();

        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                highLightTilemapLayer.SetCell(new Vector2I((int)x, (int)y), 0, Vector2I.Zero);
            }
        }
    }

    public void ClearHighlightedTiles()
    {
        highLightTilemapLayer.Clear();
    }
}
