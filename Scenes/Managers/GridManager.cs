using System.Collections.Generic;

using Godot;

using GridBasedPuzzle.Components;
using GridBasedPuzzle.Globals;

namespace GridBasedPuzzle.Managers;

public partial class GridManager : Node
{
    [Export] private TileMapLayer highlightTilemapLayer;
    [Export] private TileMapLayer baseTerrainTilemapLayer;

    private HashSet<Vector2I> occupiedCells = [];
    private HashSet<Vector2I> validBuildableTiles = [];

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
        UpdateValidBuildableTiles(b);
        MarkTileAsOccupied(b.GetRootGridCellPosition());
    }

    private void UpdateValidBuildableTiles(BuildingComponent b)
    {
        var rootCell = b.GetRootGridCellPosition();
        for (var x = rootCell.X - b.BuildableRadius; x <= rootCell.X + b.BuildableRadius; x++)
        {
            for (var y = rootCell.Y - b.BuildableRadius; y <= rootCell.Y + b.BuildableRadius; y++)
            {
                var tilePosition = new Vector2I(x, y);
                if (!IsTilePositionValid(tilePosition)) continue;
                validBuildableTiles.Add(tilePosition);
            }
        }
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
        foreach (var tilePosition in validBuildableTiles)
        {
            highlightTilemapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
        }
    }
}
