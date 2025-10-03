using Godot;

namespace GridBasedPuzzle.Core;

public partial class Main : Node2D
{
    private Sprite2D cursor;
    private Button placeBuildingButton;

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
    }

    public override void _Process(double delta)
    {
        var gridPosition = GetMouseGridCellPosition();
        cursor.GlobalPosition = gridPosition * 64;
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

    private void PlaceBuildingAtMousePosition()
    {
        var building = buildingScene.Instantiate<Node2D>();
        AddChild(building);
        var gridPosition = GetMouseGridCellPosition();
        building.GlobalPosition = gridPosition * 64;
    }
}
