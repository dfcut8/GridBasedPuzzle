using Godot;

using GridBasedPuzzle.Buildings;
using GridBasedPuzzle.Core;
using GridBasedPuzzle.Resources.Buildings;
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
    private int availableResourceCount => startingResourceCount + currentResourceCount - usedResourceCount;
    private BuildingResource toPlaceBuildingResource;
    private Vector2I hoveredGridCell;
    private Cursor cursor;
    private State currentState;


    public override void _Ready()
    {
        InitSignals();
    }

    public override void _Process(double delta)
    {
        if (!IsInstanceValid(cursor)) return;
        var gridPosition = gridManager.GetMouseGridCellPosition();
        cursor.GlobalPosition = gridPosition * 64;
        if (hoveredGridCell != gridPosition)
        {
            hoveredGridCell = gridPosition;
            UpdateHoveredGridCell();
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
        if (e.IsActionPressed(InputConstants.BUILDING_CANCEL)) ClearCursor();
        else if (toPlaceBuildingResource is not null
            && e.IsActionPressed(InputConstants.BUILDING_MOUSE_LEFT_CLICK)
            && IsBuildingPlaceableAtTile(hoveredGridCell))
        {
            PlaceBuildingAtHoveredCellPosition();
        }
    }

    private void UpdateHoveredGridCell()
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

    private void DestroyBuildingAtHoveredCellPosition()
    {

    }

    private void InitSignals()
    {
        gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;

        ui.BuildingResourceSelected += br =>
        {
            if (IsInstanceValid(cursor)) cursor.QueueFree();
            cursor = cursorScene.Instantiate<Cursor>();
            ySortRoot.AddChild(cursor);

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
        if (IsBuildingPlaceableAtTile(hoveredGridCell))
        {
            gridManager.HighlightExpandedBuildableTiles(hoveredGridCell, toPlaceBuildingResource.BuildableRadius);
            gridManager.HighlightResourceTiles(hoveredGridCell, toPlaceBuildingResource.ResourceRadius);
            cursor.SetValid();
        }
        else
        {
            cursor.SetInvalid();
        }
    }

    private void OnResourceTilesUpdated(int resourceCount)
    {
        currentResourceCount += resourceCount;
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        ySortRoot.AddChild(building);

        building.GlobalPosition = hoveredGridCell * 64;
        usedResourceCount += toPlaceBuildingResource.ResourceCost;
        GD.Print($"Used Resources: {usedResourceCount}; Available Resources: {availableResourceCount}.");
        ClearCursor();
    }

    private void ClearCursor()
    {
        gridManager.ClearHighlightedTiles();
        if (IsInstanceValid(cursor)) cursor.QueueFree();
        cursor = null;
    }

    private bool IsBuildingPlaceableAtTile(Vector2I tilePosition)
    {
        return gridManager.IsTilePositionBuildable(tilePosition)
                && availableResourceCount >= toPlaceBuildingResource.ResourceCost;
    }
}
