using System;
using System.Collections.Generic;
using System.Linq;

using Godot;

using GridBasedPuzzle.Components;
using GridBasedPuzzle.Core;

namespace GridBasedPuzzle.Managers;

public partial class GridManager : Node
{
    private const string IS_WOOD_LAYER_NAME = "IsWood";
    private const string IS_BUILDABLE_LAYER_NAME = "IsBuildable";
    private const string IS_IGNORED_LAYER_NAME = "IsIgnored";

    /// <summary>
    /// Delegate that is invoked when the set of collected resource tiles changes.
    /// The delegate should return the current count of collected resource tiles.
    /// </summary>
    /// <remarks>
    /// The <see cref="int"/> returned the number of resource tiles
    /// that have been collected/tracked (for example, wood tiles collected by
    /// buildings). Consumers can assign a function that computes or returns
    /// that count and use the value for UI updates or gameplay logic.
    /// </remarks>
    public Action<int> ResourceTilesUpdated;
    public Action GridStateUpdated;
    [Export] private TileMapLayer highlightTilemapLayer;
    [Export] private TileMapLayer baseTerrainTilemapLayer;

    private HashSet<Vector2I> validBuildableTiles = [];
    private HashSet<Vector2I> occupiedTiles = [];
    private HashSet<Vector2I> collectedResourceTiles = [];
    private List<TileMapLayer> tileMapLayers = [];

    public override void _Ready()
    {
        GlobalEvents.BuildingPlaced += OnBuildingPlaced;
        GlobalEvents.BuildingDestroyed += OnBuildingDestroyed;
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

    private void OnBuildingDestroyed(BuildingComponent bc)
    {
        RecalculateGrid(bc);
    }

    private void RecalculateGrid(BuildingComponent excludedComponent)
    {
        occupiedTiles.Clear();
        validBuildableTiles.Clear();
        collectedResourceTiles.Clear();

        var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>()
            .Except([excludedComponent]);

        foreach (var bc in buildingComponents)
        {
            UpdateValidBuildableTiles(bc);
            UpdateCollectedResourceTiles(bc);
        }
        ResourceTilesUpdated?.Invoke(collectedResourceTiles.Count);
        GridStateUpdated?.Invoke();
    }

    private void OnBuildingPlaced(BuildingComponent b)
    {
        UpdateValidBuildableTiles(b);
        UpdateCollectedResourceTiles(b);
    }

    private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
    {
        occupiedTiles.UnionWith(buildingComponent.GetOccupiedSellPositions());
        var rootCell = buildingComponent.GetRootGridCellPosition();
        var tileArea = new Rect2I(rootCell, buildingComponent.BuildingResource.Dimensions);
        var validTiles = GetTilesRadius(tileArea,
            buildingComponent.BuildingResource.BuildableRadius,
            (tilePosition) => TileHasCustomData(tilePosition, IS_BUILDABLE_LAYER_NAME));
        validBuildableTiles.UnionWith(validTiles);

        foreach (var tilePosition in occupiedTiles)
        {
            validBuildableTiles.Remove(tilePosition);
        }
        GridStateUpdated?.Invoke();
    }

    private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
    {
        var rootCell = buildingComponent.GetRootGridCellPosition();
        var tileArea = new Rect2I(rootCell, buildingComponent.BuildingResource.Dimensions);
        var resourceTiles = GetTilesRadius(tileArea,
            buildingComponent.BuildingResource.ResourceRadius,
            (tilePosition) => TileHasCustomData(tilePosition, IS_WOOD_LAYER_NAME));

        var oldResourceTileCount = collectedResourceTiles.Count;
        collectedResourceTiles.UnionWith(resourceTiles);
        if (oldResourceTileCount != collectedResourceTiles.Count)
        {
            ResourceTilesUpdated?.Invoke(collectedResourceTiles.Count);
        }
        GridStateUpdated?.Invoke();
    }

    private List<Vector2I> GetTilesRadius(Rect2I tileArea, int radius, Func<Vector2I, bool> filter)
    {
        List<Vector2I> result = [];
        for (var x = tileArea.Position.X - radius; x <= tileArea.End.X + radius; x++)
        {
            for (var y = tileArea.Position.Y - radius; y <= tileArea.End.Y + radius; y++)
            {
                var tilePosition = new Vector2I(x, y);
                if (!filter(tilePosition)) continue;
                result.Add(tilePosition);
            }
        }
        return result;
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
            if (customData is null || (bool)customData.GetCustomData(IS_IGNORED_LAYER_NAME))
            {
                continue;
            }
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
        return ConvertWorldPositionToTilePosition(highlightTilemapLayer.GetGlobalMousePosition());
    }

    public void HighlightBuildableTiles()
    {
        foreach (var tilePosition in validBuildableTiles)
        {
            highlightTilemapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
        }
    }

    public void HighlightExpandedBuildableTiles(Rect2I tileArea, int radius)
    {
        var validTiles = GetTilesRadius(tileArea, radius,
            (tilePosition) => TileHasCustomData(tilePosition, IS_BUILDABLE_LAYER_NAME))
            .ToHashSet();
        var expandedTiles = validTiles.Except(validBuildableTiles).Except(occupiedTiles);
        var atlasCoords = new Vector2I(1, 0);
        foreach (var tilePosition in expandedTiles)
        {
            highlightTilemapLayer.SetCell(tilePosition, 0, atlasCoords);
        }
    }

    public void HighlightResourceTiles(Rect2I tileArea, int radius)
    {
        var resourceTiles = GetTilesRadius(tileArea,
            radius,
            (tilePosition) => TileHasCustomData(tilePosition, IS_WOOD_LAYER_NAME));
        var atlasCoords = new Vector2I(1, 0);
        foreach (var tilePosition in resourceTiles)
        {
            highlightTilemapLayer.SetCell(tilePosition, 0, atlasCoords);
        }
    }

    public Vector2I ConvertWorldPositionToTilePosition(Vector2 worldPosition)
    {
        var gridPosition = (worldPosition / 64).Floor();
        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
    }
}
