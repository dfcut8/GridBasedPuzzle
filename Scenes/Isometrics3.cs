using Godot;

public partial class Isometrics3 : Node2D
{
    [Export] private TileMapLayer tileMap;
    [Export] private PackedScene tileAnimationSprite;

    private Node2D ySort;

    public override void _Ready()
    {
        ySort = GetNode<Node2D>("%YSort");
        var tiles = tileMap.GetUsedCells();
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        var tile = tiles[rng.RandiRange(0, tiles.Count - 1)];
        GD.Print($"selected tile: {tile}");

        // Disable the original tile sprite
        tileMap.SetCell(tile);

        // Get the world position of the target tile
        var targetPosition = tileMap.MapToLocal(tile);

        // Create sprite from packed scene
        var sprite = tileAnimationSprite.Instantiate<Sprite2D>();
        ySort.AddChild(sprite);

        // Position sprite at top of screen with same Y as target (for correct y-sort)
        var viewport = GetViewport().GetVisibleRect();
        sprite.GlobalPosition = new Vector2(targetPosition.X, targetPosition.Y);
        sprite.GlobalPosition = new Vector2(targetPosition.X, -viewport.Size.Y);

        // Create and play tween
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Quad);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(sprite, "global_position", targetPosition, 2.0f);
    }
}
