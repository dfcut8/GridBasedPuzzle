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
    }

    private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
    {
        var rootCell = buildingComponent.GetRootGridCellPosition();
        var validTiles = GetValidTilesRadius(rootCell, buildingComponent.BuildableRadius);
        validBuildableTiles.UnionWith(validTiles);


        foreach (var tilePosition in GetOccupiedTiles())
        {
            validBuildableTiles.Remove(tilePosition);
        }
    }

    private List<Vector2I> GetValidTilesRadius(Vector2I rootCell, int radius)
    {
        List<Vector2I> result = [];
        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                var tilePosition = new Vector2I(x, y);
                if (!IsTilePositionValid(tilePosition)) continue;
                result.Add(tilePosition);
            }
        }
        return result;
    }

    private IEnumerable<Vector2I> GetOccupiedTiles()
    {
        var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();
        return buildingComponents.Select(x => x.GetRootGridCellPosition());
    }

    public void ClearHighlightedTiles()
    {
        highlightTilemapLayer.Clear();
    }

    public bool IsTilePositionValid(Vector2I tilePosition)
    {
        var customData = baseTerrainTilemapLayer.GetCellTileData(tilePosition);
        if (customData is null) return false;
        return (bool)customData.GetCustomData("Buildable");
    }

    public bool IsTilePositionBuildable(Vector2I tilePosition)
    {
        return validBuildableTiles.Contains(tilePosition);
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

    public void HighlightExpandedBuildableTiles(Vector2I rootCell, int radius)
    {
        ClearHighlightedTiles();
        HighlightBuildableTiles();
        var validTiles = GetValidTilesRadius(rootCell, radius).ToHashSet();
        var expandedTiles = validTiles.Except(validBuildableTiles).Except(GetOccupiedTiles());
        var atlasCoords = new Vector2I(1, 0);
        foreach (var tilePosition in expandedTiles)
        {
            highlightTilemapLayer.SetCell(tilePosition, 0, atlasCoords);
        }
    }
}
