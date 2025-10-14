using Godot;

namespace GridBasedPuzzle.Buildings;

public partial class Building : Node2D
{
    public Sprite2D Sprite2D { get; private set; }

    public override void _Ready()
    {
        Sprite2D = GetNode<Sprite2D>("%Sprite2D");
    }
}
