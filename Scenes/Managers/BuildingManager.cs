using Godot;

using GridBasedPuzzle.Resources.Buildings;
using GridBasedPuzzle.UserInterface;

namespace GridBasedPuzzle.Managers;

public partial class BuildingManager : Node
{
    [Export] private GridManager gridManager;
    [Export] private Ui ui;
    [Export] private Node2D ySortRoot;
    [Export] private Sprite2D cursor;
    [Export] private int startingResourceCount = 4;


    private int currentResourceCount;
    private BuildingResource toPlaceBuildingResource;
    private Vector2I? hoveredGridCell;

    public override void _Ready()
    {
        currentResourceCount = startingResourceCount;

        InitCursor();
        InitSignals();
    }

    public override void _Process(double delta)
    {
        var gridPosition = gridManager.GetMouseGridCellPosition();
        cursor.GlobalPosition = gridPosition * 64;
        if (toPlaceBuildingResource is not null
            && cursor.Visible
            && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition))
        {
            hoveredGridCell = gridPosition;
            gridManager.ClearHighlightedTiles();
            gridManager.HighlightExpandedBuildableTiles(hoveredGridCell.Value, toPlaceBuildingResource.BuildableRadius);
            gridManager.HighlightResourceTiles(hoveredGridCell.Value, toPlaceBuildingResource.ResourceRadius);
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (hoveredGridCell.HasValue
            && e.IsActionPressed("mouse_click_left")
            && gridManager.IsTilePositionBuildable(hoveredGridCell.Value))
        {
            PlaceBuildingAtHoveredCellPosition();
            cursor.Visible = false;
        }
    }

    private void InitSignals()
    {
        gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;

        ui.BuildingResourceSelected += (br) =>
        {
            toPlaceBuildingResource = br;
            cursor.Visible = true;
            gridManager.HighlightBuildableTiles();
        };
    }

    private void InitCursor()
    {
        cursor = GetNode<Sprite2D>("%Cursor");
        cursor.Visible = false;
    }

    private void OnResourceTilesUpdated(int resourceCount)
    {
        GD.Print($"Resource Count: {resourceCount}");
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!hoveredGridCell.HasValue) return;

        var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        ySortRoot.AddChild(building);

        building.GlobalPosition = hoveredGridCell.Value * 64;

        hoveredGridCell = null;
        gridManager.ClearHighlightedTiles();
    }
}
