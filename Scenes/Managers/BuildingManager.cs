using System.Linq;

using Godot;

using GridBasedPuzzle.Core;
using GridBasedPuzzle.Managers;
using GridBasedPuzzle.Resources.Buildings;
using GridBasedPuzzle.Scenes.Components;
using GridBasedPuzzle.Scenes.Core;
using GridBasedPuzzle.Scenes.Ui;

using Cursor = GridBasedPuzzle.Scenes.Ui.Cursor;

namespace GridBasedPuzzle.Scenes.Managers;

public partial class BuildingManager : Node
{
    [Export] private GridManager gridManager;
    [Export] private Hud ui;
    [Export] private PackedScene cursorScene;
    [Export] private Node2D ySortRoot;

    private int startingResourceCount;

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
    private Vector2 cursorDimensions;
    private State currentState;


    public override void _Ready()
    {
        InitSignals();
        ui.Ready += () => ui.UpdateAvailableResources(AvailableResourceCount);
    }

    public override void _Process(double delta)
    {
        Vector2I gridPosition = Vector2I.Zero;

        switch (currentState)
        {
            case State.Normal:
                gridPosition = gridManager.GetMouseGridCellPosition();
                break;
            case State.PlacingBuilding:
                gridPosition = gridManager.GetCursorDimensionsWithOffset(cursorDimensions);
                cursor.GlobalPosition = gridPosition * GlobalConstants.TILE_SIZE_PIXELS;
                break;
        }

        if (hoveredGridArea.Position != gridPosition)
        {
            hoveredGridArea.Position = gridPosition;
            UpdateHoveredGridArea();
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

    public void SetStartingResourceCount(int count)
    {
        startingResourceCount = count;
        ui.UpdateAvailableResources(AvailableResourceCount);
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
        var bc = BuildingComponent.GetValidBuildingComponents(this)
            .FirstOrDefault(bc => bc.BuildingResource.IsDeletable && bc.IsTileInBuildingArea(rootCell));
        if (bc == null) return;
        if (!gridManager.CanDestroyBuilding(bc)) return;
        usedResourceCount -= bc.BuildingResource.ResourceCost;
        bc.DestroyBuilding();
        ui.UpdateAvailableResources(AvailableResourceCount);
    }

    private void InitSignals()
    {
        gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;

        ui.BuildingResourceSelected += br =>
        {
            ChangeState(State.PlacingBuilding);
            hoveredGridArea.Size = br.Dimensions;
            var buildingSprite = br.BuildingSpriteScene.Instantiate<Sprite2D>();
            cursor.SetBuildingSprite(buildingSprite);
            cursor.SetDimensions(br.Dimensions);
            cursorDimensions = br.Dimensions;

            toPlaceBuildingResource = br;
            UpdateGridDisplay();
        };
    }

    private void UpdateGridDisplay()
    {
        gridManager.ClearHighlightedTiles();

        if (toPlaceBuildingResource.IsAttackBuilding)
        {
            gridManager.HighlightGoblinOccupiedTiles();
            gridManager.HighlightBuildableTiles(true);
        }
        else
        {
            gridManager.HighlightBuildableTiles();
            gridManager.HighlightGoblinOccupiedTiles();
        }
        if (IsBuildingPlaceableAtArea(hoveredGridArea))
        {
            if (toPlaceBuildingResource.IsAttackBuilding)
            {
                gridManager.HighlightAttackTiles(hoveredGridArea, toPlaceBuildingResource.AttackRadius);
            }
            else
            {
                gridManager.HighlightExpandedBuildableTiles(hoveredGridArea, toPlaceBuildingResource.BuildableRadius);
            }

            gridManager.HighlightResourceTiles(hoveredGridArea, toPlaceBuildingResource.ResourceRadius);
            cursor.SetValid();
        }
        else
        {
            cursor.SetInvalid();
        }
        cursor.PlayHoverAnimation();
    }

    private void OnResourceTilesUpdated(int resourceCount)
    {
        currentResourceCount = resourceCount;
        ui.UpdateAvailableResources(AvailableResourceCount);
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        ySortRoot.AddChild(building);

        building.GlobalPosition = hoveredGridArea.Position * GlobalConstants.TILE_SIZE_PIXELS;
        building.GetChildren().OfType<BuildingAnimatorComponent>().FirstOrDefault()?.PlayInAnimation();
        usedResourceCount += toPlaceBuildingResource.ResourceCost;
        ui.UpdateAvailableResources(AvailableResourceCount);
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
        var buildable = gridManager.IsTileAreaBuildable(tileArea, toPlaceBuildingResource.IsAttackBuilding);
        return buildable && AvailableResourceCount >= toPlaceBuildingResource.ResourceCost;
    }
}
