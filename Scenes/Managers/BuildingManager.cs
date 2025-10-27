using System.Linq;

using Godot;

using GridBasedPuzzle.Buildings;
using GridBasedPuzzle.Components;
using GridBasedPuzzle.Core;
using GridBasedPuzzle.Resources.Buildings;
using GridBasedPuzzle.Scenes.Core;
using GridBasedPuzzle.UserInterface;

namespace GridBasedPuzzle.Managers;

public partial class BuildingManager : Node
{
    [Export] private GridManager gridManager;
    [Export] private Ui ui;
    [Export] private PackedScene cursorScene;
    [Export] private Node2D ySortRoot;
    [Export] private int startingResourceCount = 4;

    private enum State
    {
        Normal,
        PlacingBuilding
    }

    private int currentResourceCount;
    private int usedResourceCount;
    private int AvailableResourceCount => startingResourceCount + currentResourceCount - usedResourceCount;
    private BuildingResource toPlaceBuildingResource;
    private Rect2I hoveredGridArea = new(Vector2I.Zero, Vector2I.One);
    private Cursor cursor;
    private State currentState;


    public override void _Ready()
    {
        InitSignals();
        ui.Ready += () => ui.UpdateResources(usedResourceCount, AvailableResourceCount, currentResourceCount);
    }

    public override void _Process(double delta)
    {
        var gridPosition = gridManager.GetMouseGridCellPosition();
        if (hoveredGridArea.Position != gridPosition)
        {
            hoveredGridArea.Position = gridPosition;
            UpdateHoveredGridArea();
        }
        switch (currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                cursor.GlobalPosition = gridPosition * 64;
                break;
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
        switch (currentState)
        {
            case State.Normal:
                HandleNormalInput(e);
                break;
            case State.PlacingBuilding:
                HandlePlacingBuildingInput(e);
                break;
            default:
                break;
        }
    }

    private void HandleNormalInput(InputEvent e)
    {
        if (e.IsActionPressed(InputConstants.MOUSE_RIGHT_CLICK))
        {
            DestroyBuildingAtHoveredCellPosition();
        }
    }

    private void HandlePlacingBuildingInput(InputEvent e)
    {
        if (e.IsActionPressed(InputConstants.BUILDING_CANCEL)) ChangeState(State.Normal);
        else if (toPlaceBuildingResource is not null
            && e.IsActionPressed(InputConstants.BUILDING_MOUSE_LEFT_CLICK)
            && IsBuildingPlaceableAtArea(hoveredGridArea))
        {
            PlaceBuildingAtHoveredCellPosition();
        }
    }

    private void UpdateHoveredGridArea()
    {
        switch (currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                UpdateGridDisplay();
                break;
            default:
                break;
        }
    }

    private void ChangeState(State toState)
    {
        switch (currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                ClearCursor();
                toPlaceBuildingResource = null;
                break;
        }
        currentState = toState;

        switch (currentState)
        {
            case State.Normal:
                break;
            case State.PlacingBuilding:
                CreateCursor();
                break;
        }
    }

    private void CreateCursor()
    {
        cursor = cursorScene.Instantiate<Cursor>();
        ySortRoot.AddChild(cursor);
    }

    private void DestroyBuildingAtHoveredCellPosition()
    {
        var rootCell = hoveredGridArea.Position;
        var buildingComponent = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>()
            .Where(bc => bc.IsTileInBuildingArea(rootCell))
            .FirstOrDefault();
        if (buildingComponent == null) return;
        usedResourceCount -= buildingComponent.BuildingResource.ResourceCost;
        buildingComponent.DestroyBuilding();
        ui.UpdateResources(usedResourceCount, AvailableResourceCount, currentResourceCount);
    }

    private void InitSignals()
    {
        gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;

        ui.BuildingResourceSelected += br =>
        {
            ChangeState(State.PlacingBuilding);
            hoveredGridArea.Size = br.Dimensions;
            var cursorSprite = br.BuildingSpriteScene.Instantiate<Sprite2D>();
            cursor.AddChild(cursorSprite);

            toPlaceBuildingResource = br;
            UpdateGridDisplay();
        };
    }

    private void UpdateGridDisplay()
    {
        gridManager.ClearHighlightedTiles();
        gridManager.HighlightBuildableTiles();
        if (IsBuildingPlaceableAtArea(hoveredGridArea))
        {
            gridManager.HighlightExpandedBuildableTiles(hoveredGridArea, toPlaceBuildingResource.BuildableRadius);
            gridManager.HighlightResourceTiles(hoveredGridArea, toPlaceBuildingResource.ResourceRadius);
            cursor.SetValid();
        }
        else
        {
            cursor.SetInvalid();
        }
    }

    private void OnResourceTilesUpdated(int resourceCount)
    {
        currentResourceCount = resourceCount;
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        ySortRoot.AddChild(building);

        building.GlobalPosition = hoveredGridArea.Position * GlobalConstants.TILE_SIZE_PIXELS;
        usedResourceCount += toPlaceBuildingResource.ResourceCost;
        ui.UpdateResources(usedResourceCount, AvailableResourceCount, currentResourceCount);
        ChangeState(State.Normal);
    }

    private void ClearCursor()
    {
        gridManager.ClearHighlightedTiles();
        if (IsInstanceValid(cursor)) cursor.QueueFree();
        cursor = null;
    }

    private bool IsBuildingPlaceableAtArea(Rect2I tileArea)
    {
        var buildable = gridManager.IsTileAreaBuildable(tileArea);
        return buildable && AvailableResourceCount >= toPlaceBuildingResource.ResourceCost;
    }
}
