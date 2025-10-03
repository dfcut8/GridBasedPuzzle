using Godot;

namespace GridBasedPuzzle.Core;

public partial class Main : Node2D
{
    public override void _Process(double delta)
    {
        var mousePosition = GetGlobalMousePosition();
        var gridPosition = (mousePosition / 64).Floor();
        GD.Print($"Grid Position: {gridPosition}");
    }
}
