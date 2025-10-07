using Godot;

namespace GridBasedPuzzle.Managers;

public partial class GridManager : Node
{
    [Export] private TileMapLayer highLightTileMapLayer;
    [Export] private TileMapLayer baseTerrainTileMapLayer;

    public void HighlightValidTilesInRadius(Vector2 rootCell, int radius)
    {
        highLightTileMapLayer.Clear();

        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                highLightTileMapLayer.SetCell(new Vector2I((int)x, (int)y), 0, Vector2I.Zero);
            }
        }
    }
}
