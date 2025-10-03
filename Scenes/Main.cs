using Godot;

namespace GridBasedPuzzle.Core;

public partial class Main : Node2D
{
    private Sprite2D sprite;

    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("%Cursor");
    }

    public override void _Process(double delta)
    {
        var mousePosition = GetGlobalMousePosition();
        var gridPosition = (mousePosition / 64).Floor();
        GD.Print($"Grid Position: {gridPosition}");
        sprite.GlobalPosition = gridPosition * 64;
    }
}
