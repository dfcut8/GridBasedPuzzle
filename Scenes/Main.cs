using System.Collections.Generic;

using Godot;

using GridBasedPuzzle.Managers;

namespace GridBasedPuzzle.Core;

public partial class Main : Node
{
    [Export] private GridManager gridManager;
    private Sprite2D cursor;
    private Button placeBuildingButton;
    private TileMapLayer highLightTileMapLayer;
    private Vector2? hoveredGridCell;

    private HashSet<Vector2> occupiedCells = [];

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

        highLightTileMapLayer = GetNode<TileMapLayer>("%HighLightTileMapLayer");
    }

    public override void _Process(double delta)
    {
        var gridPosition = GetMouseGridCellPosition();
        cursor.GlobalPosition = gridPosition * 64;
        if (cursor.Visible && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition))
        {
            hoveredGridCell = gridPosition;
            gridManager.HighlightValidTilesInRadius(hoveredGridCell.Value, 3);
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (hoveredGridCell.HasValue && e.IsActionPressed("mouse_click_left")
            && !occupiedCells.Contains(GetMouseGridCellPosition()))
        {
            PlaceBuildingAtHoveredCellPosition();
            cursor.Visible = false;
        }
    }

    private Vector2 GetMouseGridCellPosition()
    {
        var mousePosition = highLightTileMapLayer.GetGlobalMousePosition();
        return (mousePosition / 64).Floor();
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!hoveredGridCell.HasValue) return;

        var building = buildingScene.Instantiate<Node2D>();
        AddChild(building);

        building.GlobalPosition = hoveredGridCell.Value * 64;
        occupiedCells.Add(hoveredGridCell.Value);

        hoveredGridCell = null;
        gridManager.ClearHighlightedTiles();
    }
}
