using Godot;

using GridBasedPuzzle.Buildings;
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
    private Node2D cursor;

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
            gridManager.ClearHighlightedTiles();
            gridManager.HighlightBuildableTiles();
            if (IsBuildingPlaceableAtTile(hoveredGridCell.Value))
            {
                gridManager.HighlightExpandedBuildableTiles(hoveredGridCell.Value, toPlaceBuildingResource.BuildableRadius);
                gridManager.HighlightResourceTiles(hoveredGridCell.Value, toPlaceBuildingResource.ResourceRadius);
            }
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (hoveredGridCell.HasValue
            && toPlaceBuildingResource is not null
            && e.IsActionPressed("mouse_click_left")
            && IsBuildingPlaceableAtTile(hoveredGridCell.Value))
        {
            PlaceBuildingAtHoveredCellPosition();
            cursor.Visible = false;
        }
    }

    private void InitSignals()
    {
        gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;

        ui.BuildingResourceSelected += br =>
        {
            cursor = cursorScene.Instantiate<Node2D>();
            ySortRoot.AddChild(cursor);

            cursor = br.BuildingScene.Instantiate<Building>();
            toPlaceBuildingResource = br;
            cursor.Visible = true;
            gridManager.HighlightBuildableTiles();
        };
    }

    private void OnResourceTilesUpdated(int resourceCount)
    {
        currentResourceCount += resourceCount;
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!hoveredGridCell.HasValue) return;

        var building = toPlaceBuildingResource.BuildingScene.Instantiate<Building>();
        ySortRoot.AddChild(building);

        building.GlobalPosition = hoveredGridCell.Value * 64;

        hoveredGridCell = null;
        gridManager.ClearHighlightedTiles();

        usedResourceCount += toPlaceBuildingResource.ResourceCost;
        GD.Print($"Used Resources: {usedResourceCount}; Available Resources: {availableResourceCount}.");
        cursor.QueueFree();
        cursor = null;
    }

    private bool IsBuildingPlaceableAtTile(Vector2I tilePosition)
    {
        return gridManager.IsTilePositionBuildable(tilePosition)
                && availableResourceCount >= toPlaceBuildingResource.ResourceCost;
    }
}
