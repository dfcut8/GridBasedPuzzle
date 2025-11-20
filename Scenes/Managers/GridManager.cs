using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GridBasedPuzzle.Core;
using GridBasedPuzzle.Levels.Utils;
using GridBasedPuzzle.Scenes.Components;
using GridBasedPuzzle.Scenes.Core;

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

    [Export]
    private TileMapLayer highlightTilemapLayer;

    [Export]
    private TileMapLayer baseTerrainTilemapLayer;

    private HashSet<Vector2I> allTilesInBuildableRadius = [];
    private HashSet<Vector2I> validBuildableTiles = [];
    private HashSet<Vector2I> occupiedTiles = [];
    private HashSet<Vector2I> collectedResourceTiles = [];
    private HashSet<Vector2I> goblinOccupiedTiles = [];
    private HashSet<Vector2I> validBuildableAttackTiles = [];
    private HashSet<Vector2I> attackTiles = [];
    private List<TileMapLayer> tileMapLayers = [];
    private Dictionary<TileMapLayer, ElevationLayer> tileMapLayerToElevationLayer = [];
    private Dictionary<BuildingComponent, HashSet<Vector2I>> buildingToBuildableTiles = [];

    public override void _Ready()
    {
        GlobalEvents.BuildingPlaced += OnBuildingPlaced;
        GlobalEvents.BuildingDestroyed += OnBuildingDestroyed;
        GlobalEvents.BuildingDisabled += OnBuildingDisabled;
        GlobalEvents.BuildingEnabled += OnBuildingEnabled;
        tileMapLayers = GetTileMapLayers(baseTerrainTilemapLayer);
        MapTileMapLayersToElevationLayers();

        foreach (var layer in tileMapLayers)
        {
            GD.Print(layer.Name);
        }
    }

    protected override void Dispose(bool disposing)
    {
        GlobalEvents.BuildingPlaced -= OnBuildingPlaced;
        GlobalEvents.BuildingDestroyed -= OnBuildingDestroyed;
        GlobalEvents.BuildingDisabled -= OnBuildingDisabled;
        GlobalEvents.BuildingEnabled -= OnBuildingEnabled;
    }

    public bool CanDestroyBuilding(BuildingComponent bcToDestroy)
    {
        if (bcToDestroy.BuildingResource.BuildableRadius > 0)
        {
            return IsBuildingNetworkConnected(bcToDestroy) && OrphanBuildingsCheck(bcToDestroy);
        }
        return true;
    }

    /// <summary>
    /// Validates that destroying a building won't leave other buildings without access to buildable tiles.
    /// </summary>
    /// <remarks>
    /// This method ensures that all buildings dependent on the destroyed building's buildable radius
    /// can still place themselves entirely within the buildable radii of remaining buildings.
    /// If any dependent building loses access to all required tiles, the destruction is invalid.
    /// </remarks>
    /// <param name="bcToDestroy">The building component to check for orphaning other buildings.</param>
    /// <returns>
    /// <c>true</c> if all dependent buildings can still be placed after destruction;
    /// <c>false</c> if any building would become orphaned without access to buildable tiles.
    /// </returns>
    private bool OrphanBuildingsCheck(BuildingComponent bcToDestroy)
    {
        var dependentBuildings = BuildingComponent
            .GetNonDangerComponents(this)
            .Where(bc =>
            {
                var anyTilesInRadius = bc.GetTileArea()
                    .GetTiles()
                    .Any(buildingToBuildableTiles[bcToDestroy].Contains);
                return bc != bcToDestroy && anyTilesInRadius;
            });
        var allBuildingsStillValid = dependentBuildings.All(dependentBuilding =>
        {
            var tilesForBuilding = dependentBuilding.GetTileArea().GetTiles();
            var buildingsToCheck = buildingToBuildableTiles.Keys.Where(k =>
                k != bcToDestroy && k != dependentBuilding
            );
            return tilesForBuilding.All(tilePos =>
            {
                var tileIsInSet = buildingsToCheck.Any(bc =>
                    buildingToBuildableTiles[bc].Contains(tilePos)
                );
                return tileIsInSet;
            });
        });
        if (!allBuildingsStillValid)
        {
            return false;
        }
        return true;
    }

    private void OnBuildingEnabled(BuildingComponent bc)
    {
        UpdateBuildingComponentGridState(bc);
    }

    private void OnBuildingDisabled(BuildingComponent bc)
    {
        RecalculateGrid();
    }

    private void OnBuildingDestroyed(BuildingComponent bc)
    {
        RecalculateGrid();
    }

    private void RecalculateGrid()
    {
        occupiedTiles.Clear();
        validBuildableTiles.Clear();
        validBuildableAttackTiles.Clear();
        allTilesInBuildableRadius.Clear();
        collectedResourceTiles.Clear();
        goblinOccupiedTiles.Clear();
        attackTiles.Clear();
        buildingToBuildableTiles.Clear();

        var buildingComponents = BuildingComponent.GetValidBuildingComponents(this);

        foreach (var bc in buildingComponents)
        {
            UpdateBuildingComponentGridState(bc);
        }

        CheckGoblinCampDestruction();

        ResourceTilesUpdated?.Invoke(collectedResourceTiles.Count);
        GridStateUpdated?.Invoke();
    }

    private void OnBuildingPlaced(BuildingComponent bc)
    {
        UpdateBuildingComponentGridState(bc);
        CheckGoblinCampDestruction();
    }

    private void UpdateGoblinOccupiedTiles(BuildingComponent bc)
    {
        occupiedTiles.UnionWith(bc.GetOccupiedSellPositions());

        if (bc.IsDisabled)
        {
            return;
        }

        if (bc.BuildingResource.IsDangerBuilding)
        {
            var tiles = GetTilesRadius(
                    bc.GetTileArea(),
                    bc.BuildingResource.DangerRadius,
                    (_) => true
                )
                .ToHashSet();
            tiles.ExceptWith(occupiedTiles);
            goblinOccupiedTiles.UnionWith(tiles);
        }
    }

    private bool IsBuildingNetworkConnected(BuildingComponent bcToDestroy)
    {
        var baseBuilding = BuildingComponent
            .GetDangerBuildingComponents(this)
            .FirstOrDefault(bc => bc.BuildingResource.IsBaseBuilding);
        var visitedBuildings = new HashSet<BuildingComponent>();
        VisitAllConnectedBuildings(baseBuilding, bcToDestroy, visitedBuildings);
        var totalBuildingsToVisit = BuildingComponent
            .GetDangerBuildingComponents(this)
            .Count(bc => bc.BuildingResource.BuildableRadius > 0 && bc != bcToDestroy);
        return totalBuildingsToVisit == visitedBuildings.Count;
    }

    private void VisitAllConnectedBuildings(
        BuildingComponent root,
        BuildingComponent exclude,
        HashSet<BuildingComponent> visited
    )
    {
        var dependentBuildings = BuildingComponent
            .GetNonDangerComponents(this)
            .Where(bc =>
            {
                if (bc.BuildingResource.BuildableRadius == 0)
                    return false;
                if (visited.Contains(bc))
                    return false;
                var anyTilesInRadius = bc.GetTileArea()
                    .GetTiles()
                    .Any(t => buildingToBuildableTiles[root].Contains(t));
                return bc != exclude && anyTilesInRadius;
            })
            .ToList();
        visited.UnionWith(dependentBuildings);
        foreach (var bcDependant in dependentBuildings)
        {
            VisitAllConnectedBuildings(bcDependant, exclude, visited);
        }
    }

    private void UpdateValidBuildableTiles(BuildingComponent bc)
    {
        occupiedTiles.UnionWith(bc.GetOccupiedSellPositions());
        var allTiles = GetTilesRadius(
            bc.GetTileArea(),
            bc.BuildingResource.BuildableRadius,
            (_) => true
        );
        allTilesInBuildableRadius.UnionWith(allTiles);

        if (bc.BuildingResource.BuildableRadius > 0)
        {
            var validTiles = GetTilesRadius(
                bc.GetTileArea(),
                bc.BuildingResource.BuildableRadius,
                (tilePosition) => GetTileCustomData(tilePosition, IS_BUILDABLE_LAYER_NAME).hasData
            );
            buildingToBuildableTiles[bc] = [.. validTiles];
            validBuildableTiles.UnionWith(validTiles);
            validBuildableAttackTiles.UnionWith(validTiles);
        }
        validBuildableTiles.ExceptWith(occupiedTiles);
        validBuildableTiles.ExceptWith(goblinOccupiedTiles);
        GridStateUpdated?.Invoke();
    }

    private HashSet<Vector2I> GetBuildableTiles(bool isAttackTiles = false)
    {
        return isAttackTiles ? validBuildableAttackTiles : validBuildableTiles;
    }

    private void RecalculateGoblinOccupiedTiles()
    {
        goblinOccupiedTiles.Clear();
        var dangerBuildings = BuildingComponent.GetDangerBuildingComponents(this);
        foreach (var bc in dangerBuildings)
        {
            UpdateGoblinOccupiedTiles(bc);
        }
    }

    private void CheckGoblinCampDestruction()
    {
        var dangerBuildings = BuildingComponent.GetDangerBuildingComponents(this);
        foreach (var bc in dangerBuildings)
        {
            var tileArea = bc.GetTileArea();
            var isInsideAttackTile = tileArea.GetTiles().Any(attackTiles.Contains);
            if (isInsideAttackTile)
            {
                bc.Disable();
            }
            else
            {
                bc.Enable();
            }
        }
    }

    private void UpdateBuildingComponentGridState(BuildingComponent bc)
    {
        UpdateGoblinOccupiedTiles(bc);
        UpdateValidBuildableTiles(bc);
        UpdateCollectedResourceTiles(bc);
        UpdateAttackTiles(bc);
    }

    private void UpdateAttackTiles(BuildingComponent bc)
    {
        if (!bc.BuildingResource.IsAttackBuilding)
        {
            return;
        }

        var newAttackTiles = GetTilesRadius(
                bc.GetTileArea(),
                bc.BuildingResource.AttackRadius,
                (_) => true
            )
            .ToHashSet();
        attackTiles.UnionWith(newAttackTiles);
    }

    private void UpdateCollectedResourceTiles(BuildingComponent bc)
    {
        var resourceTiles = GetTilesRadius(
            bc.GetTileArea(),
            bc.BuildingResource.ResourceRadius,
            (tilePosition) => GetTileCustomData(tilePosition, IS_WOOD_LAYER_NAME).hasData
        );

        var oldResourceTileCount = collectedResourceTiles.Count;
        collectedResourceTiles.UnionWith(resourceTiles);
        if (oldResourceTileCount != collectedResourceTiles.Count)
        {
            ResourceTilesUpdated?.Invoke(collectedResourceTiles.Count);
        }
        GridStateUpdated?.Invoke();
    }

    private bool IsTileInsideRadius(Vector2 center, Vector2 tilePosition, float radius)
    {
        // We need to get center of the tiles in float
        var distanceX = center.X - (tilePosition.X + 0.5);
        var distanceY = center.Y - (tilePosition.Y + 0.5);

        var distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
        return distanceSquared <= radius * radius;
    }

    private List<Vector2I> GetTilesRadius(Rect2I tileArea, int radius, Func<Vector2I, bool> filter)
    {
        var rect = tileArea.ToRect();
        var tileCenter = rect.GetCenter();

        // To offset building size itself
        var offset = Mathf.Max(tileArea.Size.X, tileArea.Size.Y) / 2;

        List<Vector2I> result = [];
        for (var x = tileArea.Position.X - radius; x <= tileArea.End.X + radius; x++)
        {
            for (var y = tileArea.Position.Y - radius; y <= tileArea.End.Y + radius; y++)
            {
                var tilePosition = new Vector2I(x, y);
                if (
                    !IsTileInsideRadius(tileCenter, tilePosition, radius + offset)
                    || !filter(tilePosition)
                )
                {
                    continue;
                }
                result.Add(tilePosition);
            }
        }
        return result;
    }

    private List<TileMapLayer> GetTileMapLayers(Node2D rootNode)
    {
        // TODO: Think about doing this in loop and benchmark it...
        List<TileMapLayer> result = [];
        var children = rootNode.GetChildren();
        children.Reverse();
        foreach (var child in children)
        {
            if (child is Node2D node)
            {
                result.AddRange(GetTileMapLayers(node));
            }
        }
        if (rootNode is TileMapLayer layer)
        {
            result.Add(layer);
        }
        return result;
    }

    // Can have bugs, need to confirm works as expected
    private void MapTileMapLayersToElevationLayers()
    {
        foreach (var layer in tileMapLayers)
        {
            var elevationLayer = layer.GetParentOrNull<ElevationLayer>();
            if (elevationLayer is not null)
            {
                tileMapLayerToElevationLayer[layer] = elevationLayer;
            }
        }
        DebugTileMapLayerToElevationLayer();
    }

    private void DebugTileMapLayerToElevationLayer()
    {
        foreach (var pair in tileMapLayerToElevationLayer)
        {
            GD.Print($"Key: {pair.Key.Name}, Value: {pair.Value.Name}");
        }
    }

    public void ClearHighlightedTiles()
    {
        highlightTilemapLayer.Clear();
    }

    public (TileMapLayer layer, bool hasData) GetTileCustomData(
        Vector2I tilePosition,
        string dataName
    )
    {
        foreach (var layer in tileMapLayers)
        {
            var customData = layer.GetCellTileData(tilePosition);
            if (customData is null || (bool)customData.GetCustomData(IS_IGNORED_LAYER_NAME))
            {
                continue;
            }
            return (layer, (bool)customData.GetCustomData(dataName));
        }
        return (null, false);
    }

    public bool IsTilePositionBuildable(Vector2I tilePosition)
    {
        return validBuildableTiles.Contains(tilePosition);
    }

    public bool IsTileAreaBuildable(Rect2I rect, bool isAttackTiles = false)
    {
        var tiles = rect.GetTiles();
        if (tiles.Count == 0)
            return false;

        (TileMapLayer firstTileMapLayer, _) = GetTileCustomData(tiles[0], IS_BUILDABLE_LAYER_NAME);
        ElevationLayer targetElevationLayer = null;
        if (firstTileMapLayer != null)
        {
            tileMapLayerToElevationLayer.TryGetValue(firstTileMapLayer, out targetElevationLayer);
        }

        var tilesToCheck = GetBuildableTiles(isAttackTiles);
        if (isAttackTiles)
        {
            tilesToCheck = tilesToCheck.Except(occupiedTiles).ToHashSet();
        }

        return tiles.All(t =>
        {
            (TileMapLayer layer, bool isBuildable) = GetTileCustomData(t, IS_BUILDABLE_LAYER_NAME);
            ElevationLayer elevationLayer = null;
            if (layer != null)
            {
                tileMapLayerToElevationLayer.TryGetValue(layer, out elevationLayer);
            }

            return isBuildable
                && tilesToCheck.Contains(t)
                && elevationLayer == targetElevationLayer;
        });
    }

    public Vector2I GetMouseGridCellPosition()
    {
        return ConvertWorldPositionToTilePosition(highlightTilemapLayer.GetGlobalMousePosition());
    }

    public Vector2I GetCursorDimensionsWithOffset(Vector2 dimensions)
    {
        var mousePos =
            highlightTilemapLayer.GetGlobalMousePosition() / GlobalConstants.TILE_SIZE_PIXELS;
        GD.Print(mousePos);
        mousePos -= dimensions / 2;
        mousePos = mousePos.Round();
        var result = new Vector2I((int)mousePos.X, (int)mousePos.Y);
        GD.Print($"Cursor grid cell position with offset: {result}");
        return result;
    }

    public void HighlightGoblinOccupiedTiles()
    {
        var atlasCoords = new Vector2I(2, 0);
        foreach (var tilePosition in goblinOccupiedTiles)
        {
            highlightTilemapLayer.SetCell(tilePosition, 0, atlasCoords);
        }
    }

    public void HighlightBuildableTiles(bool isAttackTiles = false)
    {
        foreach (var tilePosition in GetBuildableTiles(isAttackTiles))
        {
            highlightTilemapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
        }
    }

    public void HighlightExpandedBuildableTiles(Rect2I tileArea, int radius)
    {
        var validTiles = GetTilesRadius(
                tileArea,
                radius,
                (tilePosition) => GetTileCustomData(tilePosition, IS_BUILDABLE_LAYER_NAME).hasData
            )
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
        var resourceTiles = GetTilesRadius(
            tileArea,
            radius,
            (tilePosition) => GetTileCustomData(tilePosition, IS_WOOD_LAYER_NAME).hasData
        );
        var atlasCoords = new Vector2I(1, 0);
        foreach (var tilePosition in resourceTiles)
        {
            highlightTilemapLayer.SetCell(tilePosition, 0, atlasCoords);
        }
    }

    public void HighlightAttackTiles(Rect2I tileArea, int radius)
    {
        var buildableAreaTiles = tileArea.GetTiles().ToHashSet();
        var tiles = GetTilesRadius(
                tileArea,
                radius,
                (tilePosition) => GetTileCustomData(tilePosition, IS_BUILDABLE_LAYER_NAME).hasData
            )
            .ToHashSet()
            .Except(validBuildableAttackTiles)
            .Except(buildableAreaTiles);
        var atlasCoords = new Vector2I(1, 0);
        foreach (var tilePosition in tiles)
        {
            highlightTilemapLayer.SetCell(tilePosition, 0, atlasCoords);
        }
    }

    public Vector2I ConvertWorldPositionToTilePosition(Vector2 worldPosition)
    {
        var gridPosition = (worldPosition / 64).Floor();
        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
    }

    public bool IsTilePositionInAnyBuildingRadius(Vector2I pos)
    {
        return allTilesInBuildableRadius.Contains(pos);
    }
}
