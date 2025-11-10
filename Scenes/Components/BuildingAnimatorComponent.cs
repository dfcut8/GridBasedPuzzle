using Godot;
using System;

namespace GridBasedPuzzle.Scenes.Components;

public partial class BuildingAnimatorComponent : Node2D
{
    public Action DestroyAnimationFinished;

    private Tween activeTween;
    private Node2D animationRoot;

    override public void _Ready()
    {
        Setup();
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
            .SetTrans(Tween.TransitionType.Bounce)
            .SetEase(Tween.EaseType.Out);
    }

    public void PlayDestroyAnimation()
    {
        if (animationRoot == null)
        {
            GD.PushError("Animation root is null!");
            return;
        }
        if (activeTween != null && activeTween.IsValid())
        {
            activeTween.Kill();
        }
        activeTween = CreateTween();
        activeTween.TweenProperty(animationRoot,
            "rotation_degrees",
            -5,
            0.1f);
        activeTween.TweenProperty(animationRoot,
            "rotation_degrees",
            -5,
            0.1f);
        activeTween.TweenProperty(animationRoot,
            "rotation_degrees",
            -2,
            0.1f);
        activeTween.TweenProperty(animationRoot,
            "rotation_degrees",
            2,
            0.1f);
        activeTween.TweenProperty(animationRoot,
            "rotation_degrees",
            0,
            0.1f);
        activeTween.TweenProperty(animationRoot,
            "position",
            Vector2.Down * 300,
            0.4f);
        activeTween.Finished += () => DestroyAnimationFinished?.Invoke();
    }

    /// <summary>
    /// Needed to setup properly for y-sort alignment.
    /// </summary>
    // Dont like this, but fine for now.
    private void Setup()
    {
        var spriteToAnimate = GetChildOrNull<Sprite2D>(0);
        if (spriteToAnimate == null)
        {
            GD.PushError("Sprite to animate should be child to component");
            return;
        }
        // need to offset position by sprite height to align bottom
        animationRoot = new Node2D();
        AddChild(animationRoot);
        Position = new Vector2(Position.X, spriteToAnimate.Position.Y);
        RemoveChild(spriteToAnimate);
        animationRoot.AddChild(spriteToAnimate);
        spriteToAnimate.Position = new Vector2(spriteToAnimate.Position.X, 0f);
    }
}
