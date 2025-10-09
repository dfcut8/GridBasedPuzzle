using System.Collections.Generic;
using System.Linq;

using Godot;

using GridBasedPuzzle.Components;
using GridBasedPuzzle.Globals;

namespace GridBasedPuzzle.Managers;

public partial class GridManager : Node
{
    [Export] private TileMapLayer highlightTilemapLayer;
    [Export] private TileMapLayer baseTerrainTilemapLayer;

    private HashSet<Vector2I> occupiedCells = [];

    public override void _Ready()
    {
        GlobalEvents.BuildingPlaced += OnBuildingPlaced;
    }

    protected override void Dispose(bool disposing)
    {
        GlobalEvents.BuildingPlaced -= OnBuildingPlaced;
    }

    private void OnBuildingPlaced(BuildingComponent b)
    {
        MarkTileAsOccupied(b.GetRootGridCellPosition());
    }

    private void HighlightValidTilesInRadius(Vector2I rootCell, int radius)
    {
        var highLightedTilesCount = 0;
        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                var tilePosition = new Vector2I(x, y);
                if (!IsTilePositionValid(tilePosition)) continue;
                highlightTilemapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
                highLightedTilesCount++;
            }
        }
        // GD.Print(highLightedTilesCount);
    }

    public void ClearHighlightedTiles()
    {
        highlightTilemapLayer.Clear();
    }

    public void MarkTileAsOccupied(Vector2I tilePosition)
    {
        occupiedCells.Add(tilePosition);
    }

    public bool IsTilePositionValid(Vector2I tilePosition)
    {
        var customData = baseTerrainTilemapLayer.GetCellTileData(tilePosition);
        if (customData is null) return false;
        if (!(bool)customData.GetCustomData("Buildable")) return false;

        return !occupiedCells.Contains(tilePosition);
    }

    public Vector2I GetMouseGridCellPosition()
    {
        var mousePosition = highlightTilemapLayer.GetGlobalMousePosition();
        var gridPosition = (mousePosition / 64).Floor();

        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
    }

    public void HighlightBuildableTiles()
    {
        ClearHighlightedTiles();
        var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();

        foreach (var b in buildingComponents)
        {
            HighlightValidTilesInRadius(b.GetRootGridCellPosition(), b.BuildableRadius);
        }
    }
}
