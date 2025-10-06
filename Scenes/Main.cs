using Godot;

namespace GridBasedPuzzle.Core;

public partial class Main : Node2D
{
    private Sprite2D cursor;
    private Button placeBuildingButton;
    private TileMapLayer highLightTileMapLayer;
    private Vector2? hoveredGridCell;

    [Export] private PackedScene buildingScene;

    public override void _Ready()
    {
        cursor = GetNode<Sprite2D>("%Cursor");
        cursor.Visible = false;
        placeBuildingButton = GetNode<Button>("%PlaceBuildingButton");
        placeBuildingButton.Pressed += () =>
        {
            GD.Print("Pressed!!!");
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
            UpdateHighLightTileMapLayer();
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (cursor.Visible && e.IsActionPressed("mouse_click_left"))
        {
            PlaceBuildingAtMousePosition();
            cursor.Visible = false;
        }
    }

    private Vector2 GetMouseGridCellPosition()
    {
        var mousePosition = GetGlobalMousePosition();
        return (mousePosition / 64).Floor();
    }

    private void UpdateHighLightTileMapLayer()
    {
        highLightTileMapLayer.Clear();
        if (!hoveredGridCell.HasValue) return;

        for (var x = hoveredGridCell.Value.X - 3; x <= hoveredGridCell.Value.X + 3; x++)
        {
            for (var y = hoveredGridCell.Value.Y - 3; y <= hoveredGridCell.Value.Y + 3; y++)
            {
                highLightTileMapLayer.SetCell(new Vector2I((int)x, (int)y), 0, Vector2I.Zero);
            }
        }
    }

    private void PlaceBuildingAtMousePosition()
    {
        var building = buildingScene.Instantiate<Node2D>();
        AddChild(building);
        var gridPosition = GetMouseGridCellPosition();
        building.GlobalPosition = gridPosition * 64;

        hoveredGridCell = null;
        UpdateHighLightTileMapLayer();
    }
}
