using System.Collections.Generic;

using Godot;

namespace GridBasedPuzzle.Managers;

public partial class GridManager : Node
{
    [Export] private TileMapLayer highlightTilemapLayer;
    [Export] private TileMapLayer baseTerrainTilemapLayer;

    private HashSet<Vector2> occupiedCells = [];

    public void HighlightValidTilesInRadius(Vector2 rootCell, int radius)
    {
        var highLightedTilesCount = 0;
        ClearHighlightedTiles();

        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                if (!IsTilePositionValid(new Vector2(x, y))) continue;
                highlightTilemapLayer.SetCell(new Vector2I((int)x, (int)y), 0, Vector2I.Zero);
                highLightedTilesCount++;
            }
        }
        GD.Print(highLightedTilesCount);
    }

    public void ClearHighlightedTiles()
    {
        highlightTilemapLayer.Clear();
    }

    public void MarkTileAsOccupied(Vector2 tilePosition)
    {
        occupiedCells.Add(tilePosition);
    }

    public bool IsTilePositionValid(Vector2 tilePosition)
    {
        return !occupiedCells.Contains(tilePosition);
    }

    public Vector2 GetMouseGridCellPosition()
    {
        var mousePosition = highlightTilemapLayer.GetGlobalMousePosition();
        return (mousePosition / 64).Floor();
    }
}
