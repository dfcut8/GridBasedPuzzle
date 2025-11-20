using System.Collections.Generic;
using Godot;

public partial class Isometrics2 : Node2D
{
    private string tweenPropertyName = "offset";

    public override void _Ready()
    {
        var camera = GetNode<Camera2D>("Camera2D");
        var viewportHeight = GetViewport().GetVisibleRect().Size.Y;
        var startY = camera.GlobalPosition.Y - viewportHeight / 2 - 600;

        // Get all Sprite2D objects from the scene tree
        var allSprites = GetAllSprites(GetTree().Root);

        // Sort sprites by Y position (top to bottom), then by X position (left to right)
        allSprites.Sort(
            (a, b) =>
            {
                var xComparison = a.GlobalPosition.X.CompareTo(b.GlobalPosition.X);
                if (xComparison != 0)
                    return xComparison;
                return a.GlobalPosition.Y.CompareTo(b.GlobalPosition.Y);
            }
        );

        float delayBetweenTweens = 0.6f;
        int spriteIndex = 0;

        foreach (var sprite in allSprites)
        {
            sprite.Offset = new Vector2(0, startY);

            var delay = spriteIndex * delayBetweenTweens;
            GetTree().CreateTimer(delay).Timeout += () =>
            {
                var tween = GetTree().CreateTween();
                tween.SetTrans(Tween.TransitionType.Quad);
                tween.SetEase(Tween.EaseType.Out);
                tween.TweenProperty(sprite, tweenPropertyName, new Vector2(0, -128f), 2.0f);
            };

            spriteIndex++;
        }
    }

    private List<Sprite2D> GetAllSprites(Node node)
    {
        var sprites = new List<Sprite2D>();

        if (node is Sprite2D sprite)
        {
            sprites.Add(sprite);
        }

        foreach (var child in node.GetChildren())
        {
            sprites.AddRange(GetAllSprites(child));
        }

        return sprites;
    }
}
