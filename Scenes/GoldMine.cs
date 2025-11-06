using Godot;

namespace GridBasedPuzzle.Scenes;

public partial class GoldMine : Node2D
{
    [Export] private Texture2D activeTexture;

    public bool IsActive { get; private set; }

    private Sprite2D sprite;

    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("Sprite2D");
    }

    public void SetActive()
    {
        sprite.Texture = activeTexture;
    }
}
