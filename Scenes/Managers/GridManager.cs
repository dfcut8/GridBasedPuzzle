using System;
using System.Collections.Generic;
using System.Linq;

using Godot;

using GridBasedPuzzle.Components;
using GridBasedPuzzle.Globals;

namespace GridBasedPuzzle.Managers;

public partial class GridManager : Node
{
    private const string IS_WOOD_LAYER_NAME = "IsWood";
    private const string IS_BUILDABLE_LAYER_NAME = "IsBuildable";
    [Export] private TileMapLayer highlightTilemapLayer;
    [Export] private TileMapLayer baseTerrainTilemapLayer;

    private HashSet<Vector2I> validBuildableTiles = [];
    private List<TileMapLayer> tileMapLayers = [];

    public override void _Ready()
    {
        GlobalEvents.BuildingPlaced += OnBuildingPlaced;
        tileMapLayers = GetTileMapLayers(baseTerrainTilemapLayer);

        foreach (var layer in tileMapLayers)
        {
            GD.Print(layer.Name);
        }
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
        var validTiles = GetTilesRadius(rootCell,
            buildingComponent.BuildingResource.BuildableRadius,
            (tilePosition) => TileHasCustomData(tilePosition, IS_BUILDABLE_LAYER_NAME));
        validBuildableTiles.UnionWith(validTiles);

        foreach (var tilePosition in GetOccupiedTiles())
        {
            validBuildableTiles.Remove(tilePosition);
        }
    }

    private List<Vector2I> GetTilesRadius(Vector2I rootCell, int radius, Func<Vector2I, bool> filter)
    {
        List<Vector2I> result = [];
        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                var tilePosition = new Vector2I(x, y);
                if (!filter(tilePosition)) continue;
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

    private List<TileMapLayer> GetTileMapLayers(TileMapLayer baseTileMapLayer)
    {
        // TODO: Think about doing this in loop and benchmark it...
        List<TileMapLayer> result = [];
        var children = baseTileMapLayer.GetChildren();
        children.Reverse();
        foreach (var child in children)
        {
            if (child is TileMapLayer layer)
            {
                result.AddRange(GetTileMapLayers(layer));
            }
        }
        result.Add(baseTileMapLayer);
        return result;
    }

    public void ClearHighlightedTiles()
    {
        highlightTilemapLayer.Clear();
    }

    public bool TileHasCustomData(Vector2I tilePosition, string dataName)
    {
        foreach (var layer in tileMapLayers)
        {
            var customData = layer.GetCellTileData(tilePosition);
            if (customData is null) continue;
            return (bool)customData.GetCustomData(dataName);
        }
        return false;
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
        HighlightBuildableTiles();
        var validTiles = GetTilesRadius(rootCell,
            radius,
            (tilePosition) => TileHasCustomData(tilePosition, IS_BUILDABLE_LAYER_NAME)).ToHashSet();
        var expandedTiles = validTiles.Except(validBuildableTiles).Except(GetOccupiedTiles());
        var atlasCoords = new Vector2I(1, 0);
        foreach (var tilePosition in expandedTiles)
        {
            highlightTilemapLayer.SetCell(tilePosition, 0, atlasCoords);
        }
    }

    public void HighlightResourceTiles(Vector2I rootCell, int radius)
    {
        var resourceTiles = GetTilesRadius(rootCell,
            radius,
            (tilePosition) => TileHasCustomData(tilePosition, IS_WOOD_LAYER_NAME));
        var atlasCoords = new Vector2I(1, 0);
        foreach (var tilePosition in resourceTiles)
        {
            highlightTilemapLayer.SetCell(tilePosition, 0, atlasCoords);
        }
    }
}
