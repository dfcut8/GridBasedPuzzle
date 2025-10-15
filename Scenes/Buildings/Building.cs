using Godot;

namespace GridBasedPuzzle.Buildings;

public partial class Building : Node2D
{
    public Sprite2D Sprite2D { get; private set; }

    public override void _Ready()
    {
        Sprite2D = GetNode<Sprite2D>("%Sprite2D");
    }

    public void SetInvalid()
    {
        Sprite2D.Modulate = Colors.Red;
    }

    public void SetSelected()
    {
        Sprite2D.Modulate = new Color(1f, 1f, 1f, 0.5137255f);
    }

    public void SetPlaced()
    {
        Sprite2D.Modulate = Colors.White;
    }
}
