using Godot;

namespace GridBasedPuzzle.Scenes.Components;

public partial class BuildingAnimatorComponent : Node2D
{
    private Tween activeTween;
    private Node2D animationRoot;

    override public void _Ready()
    {
        animationRoot = GetNode<Node2D>("%AnimationRoot");
        Setup();
        PlayInAnimation();
    }

    public void PlayInAnimation()
    {
        if (activeTween != null && activeTween.IsValid())
        {
            activeTween.Kill();
        }

        activeTween = CreateTween();
        activeTween.TweenProperty(animationRoot,
            "position",
            Vector2.Zero,
            0.5f)
            .From(Vector2.Up * 128)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }

    /// <summary>
    /// Needed to setup properly for y-sort alignment.
    /// </summary>
    private void Setup()
    {
        var spriteToAnimate = GetChildOrNull<Sprite2D>(0);
        if (spriteToAnimate == null)
        {
            GD.PushError("Sprite to animate is not assigned in BuildingAnimatorComponent.");
            return;
        }
        // need to offset position by sprite height to align bottom
        Position = new Vector2(Position.X, spriteToAnimate.Position.Y);
        RemoveChild(spriteToAnimate);
        animationRoot.AddChild(spriteToAnimate);
        spriteToAnimate.Position = new Vector2(spriteToAnimate.Position.X, 0f);
    }
}
