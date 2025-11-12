using Godot;
using System;

namespace GridBasedPuzzle.Scenes.Components;

public partial class BuildingAnimatorComponent : Node2D
{
    public Action DestroyAnimationFinished;

    [Export] private Texture2D maskTexture;

    private Tween activeTween;
    private Node2D animationRoot;
    private Sprite2D maskNode;

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
        maskNode.Texture = maskTexture;
        maskNode.ClipChildren = ClipChildrenMode.Only;
        if (animationRoot == null)
        {
            GD.PushError("Animation root is null!");
            return;
        }
        if (activeTween != null && activeTween.IsValid())
        {
            activeTween.Kill();
        }

        animationRoot.Position = Vector2.Zero;

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
            0.4f)
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.In);
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

        maskNode = new Sprite2D()
        {
            Centered = false,
            Offset = new Vector2(-160f, -256f),
        };
        AddChild(maskNode);

        // need to offset position by sprite height to align bottom
        animationRoot = new Node2D();
        maskNode.AddChild(animationRoot);
        Position = new Vector2(spriteToAnimate.Position.X, spriteToAnimate.Position.Y);
        RemoveChild(spriteToAnimate);
        animationRoot.AddChild(spriteToAnimate);
        spriteToAnimate.Position = new Vector2(0f, 0f);
    }
}
