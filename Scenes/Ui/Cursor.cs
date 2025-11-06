using Godot;
using GridBasedPuzzle.Scenes.Core;

namespace GridBasedPuzzle.Scenes.Ui;

public partial class Cursor : Node2D
{
    private Node2D topLeft;
    private Node2D topRight;
    private Node2D bottomLeft;
    private Node2D bottomRight;
    private Node2D buildingSpriteRoot;

    public override void _Ready()
    {
        topLeft = GetNode<Node2D>("TopLeft");
        topRight = GetNode<Node2D>("TopRight");
        bottomLeft = GetNode<Node2D>("BottomLeft");
        bottomRight = GetNode<Node2D>("BottomRight");
        buildingSpriteRoot = GetNode<Node2D>("BuildingSpriteRoot");
    }
    
    public void SetInvalid()
    {
        Modulate = Colors.Red;
    }

    public void SetValid()
    {
        Modulate = Colors.White;
    }

    public void SetDimensions(Vector2I dim)
    {
        bottomLeft.Position = dim * new Vector2I(0, GlobalConstants.TILE_SIZE_PIXELS);
        bottomRight.Position = dim * new Vector2I(GlobalConstants.TILE_SIZE_PIXELS, GlobalConstants.TILE_SIZE_PIXELS);
        topRight.Position = dim * new Vector2I(GlobalConstants.TILE_SIZE_PIXELS, 0);
    }

    public void SetBuildingSprite(Node2D sprite)
    {
        buildingSpriteRoot.AddChild(sprite);
    }
}
