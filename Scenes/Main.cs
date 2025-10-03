using Godot;

namespace GridBasedPuzzle.Core;

public partial class Main : Node2D
{
    private Sprite2D sprite;

    [Export] private PackedScene buildingScene;

    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("%Cursor");
    }

    public override void _Process(double delta)
    {
        var gridPosition = GetMouseGridCellPosition();
        sprite.GlobalPosition = gridPosition * 64;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("mouse_click_left"))
        {
            PlaceBuildingAtMousePosition();
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
