using Godot;

using GridBasedPuzzle.Managers;

namespace GridBasedPuzzle.Core;

public partial class Main : Node
{
    [Export] private GridManager gridManager;
    private Sprite2D cursor;
    private Button placeBuildingButton;
    private Vector2I? hoveredGridCell;

    [Export] private PackedScene buildingScene;

    public override void _Ready()
    {
        cursor = GetNode<Sprite2D>("%Cursor");
        cursor.Visible = false;
        placeBuildingButton = GetNode<Button>("%PlaceBuildingButton");
        placeBuildingButton.Pressed += () =>
        {
            cursor.Visible = true;
        };
    }

    public override void _Process(double delta)
    {
        var gridPosition = gridManager.GetMouseGridCellPosition();
        cursor.GlobalPosition = gridPosition * 64;
        if (cursor.Visible && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition))
        {
            hoveredGridCell = gridPosition;
            gridManager.HighlightBuildableTiles();
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (hoveredGridCell.HasValue && e.IsActionPressed("mouse_click_left")
            && gridManager.IsTilePositionValid(hoveredGridCell.Value))
        {
            PlaceBuildingAtHoveredCellPosition();
            cursor.Visible = false;
        }
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!hoveredGridCell.HasValue) return;

        var building = buildingScene.Instantiate<Node2D>();
        AddChild(building);

        building.GlobalPosition = hoveredGridCell.Value * 64;
        gridManager.MarkTileAsOccupied(hoveredGridCell.Value);

        hoveredGridCell = null;
        gridManager.ClearHighlightedTiles();
    }
}
