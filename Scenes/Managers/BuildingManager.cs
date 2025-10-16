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


    private int currentResourceCount;
    private int usedResourceCount;
    private int availableResourceCount => startingResourceCount + currentResourceCount - usedResourceCount;
    private BuildingResource toPlaceBuildingResource;
    private Vector2I? hoveredGridCell;
    private Cursor cursor;

    public override void _Ready()
    {
        InitSignals();
    }

    public override void _Process(double delta)
    {
        if (!IsInstanceValid(cursor)) return;

        var gridPosition = gridManager.GetMouseGridCellPosition();
        cursor.GlobalPosition = gridPosition * 64;
        if (toPlaceBuildingResource is not null
            && cursor.Visible
            && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition))
        {
            hoveredGridCell = gridPosition;
            UpdateGridDisplay();
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed(InputConstants.BUILDING_CANCEL)) ClearCursor();
        else if (hoveredGridCell.HasValue
            && toPlaceBuildingResource is not null
            && e.IsActionPressed(InputConstants.BUILDING_MOUSE_LEFT_CLICK)
            && IsBuildingPlaceableAtTile(hoveredGridCell.Value))
        {
            PlaceBuildingAtHoveredCellPosition();
        }
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
            UpdateGridDisplay();
            toPlaceBuildingResource = br;
            cursor.Visible = true;
            gridManager.HighlightBuildableTiles();
        };
    }

    private void UpdateGridDisplay()
    {
        if (!hoveredGridCell.HasValue) return;
        gridManager.ClearHighlightedTiles();
        gridManager.HighlightBuildableTiles();
        if (IsBuildingPlaceableAtTile(hoveredGridCell.Value))
        {
            gridManager.HighlightExpandedBuildableTiles(hoveredGridCell.Value, toPlaceBuildingResource.BuildableRadius);
            gridManager.HighlightResourceTiles(hoveredGridCell.Value, toPlaceBuildingResource.ResourceRadius);
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
        if (!hoveredGridCell.HasValue) return;

        var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        ySortRoot.AddChild(building);

        building.GlobalPosition = hoveredGridCell.Value * 64;
        usedResourceCount += toPlaceBuildingResource.ResourceCost;
        GD.Print($"Used Resources: {usedResourceCount}; Available Resources: {availableResourceCount}.");
        ClearCursor();
    }

    private void ClearCursor()
    {
        hoveredGridCell = null;
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
