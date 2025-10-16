using Godot;

namespace GridBasedPuzzle.Buildings;

public partial class Cursor : Node2D
{
    public void SetInvalid()
    {
        Modulate = Colors.Red;
    }

    public void SetValid()
    {
        Modulate = Colors.White;
    }
}
