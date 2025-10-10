using Godot;

using GridBasedPuzzle.Managers;

namespace GridBasedPuzzle.Core;

public partial class Main : Node
{
    [Export] private GridManager gridManager;
    [Export] private Node2D ySortRoot;
    private Sprite2D cursor;
    private Button placeTowerButton;
    private Button placeVillageButton;
    private Vector2I? hoveredGridCell;


    [Export] private PackedScene towerScene;
    [Export] private PackedScene villageScene;

    private PackedScene toPlaceScene;

    public override void _Ready()
    {
        cursor = GetNode<Sprite2D>("%Cursor");
        cursor.Visible = false;

        placeTowerButton = GetNode<Button>("%PlaceTowerButton");
        placeTowerButton.Pressed += () =>
        {
            toPlaceScene = towerScene;
            cursor.Visible = true;
            gridManager.HighlightBuildableTiles();
        };

        placeVillageButton = GetNode<Button>("%PlaceVillageButton");
        placeVillageButton.Pressed += () =>
        {
            toPlaceScene = villageScene;
            cursor.Visible = true;
            gridManager.HighlightBuildableTiles();
        };
    }

    public override void _Process(double delta)
    {
        var gridPosition = gridManager.GetMouseGridCellPosition();
        cursor.GlobalPosition = gridPosition * 64;
        if (cursor.Visible && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition))
        {
            hoveredGridCell = gridPosition;
            gridManager.HighlightExpandedBuildableTiles(hoveredGridCell.Value, 3);
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (hoveredGridCell.HasValue && e.IsActionPressed("mouse_click_left")
            && gridManager.IsTilePositionBuildable(hoveredGridCell.Value))
        {
            PlaceBuildingAtHoveredCellPosition();
            cursor.Visible = false;
        }
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!hoveredGridCell.HasValue) return;

        var building = toPlaceScene.Instantiate<Node2D>();
        ySortRoot.AddChild(building);

        building.GlobalPosition = hoveredGridCell.Value * 64;

        hoveredGridCell = null;
        gridManager.ClearHighlightedTiles();
    }
}
