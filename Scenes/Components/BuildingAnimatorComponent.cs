using Godot;
using System.Linq;

namespace GridBasedPuzzle.Scenes.Components;

public partial class BuildingAnimatorComponent : Node2D
{
    private Tween activeTween;

    override public void _Ready()
    {
        PlayInAnimation();
    }

    public void PlayInAnimation()
    {
        if (activeTween != null && activeTween.IsValid())
        {
            activeTween.Kill();
        }

        activeTween = CreateTween();
        activeTween.TweenProperty(this,
            "position",
            Vector2.Zero,
            0.5f)
            .From(Vector2.Up * 128)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }

    private void SetupNodes()
    {
        var spriteNode = GetChildren().FirstOrDefault() as Node2D;
        if (spriteNode == null)
        {
            return;
        }
    }
}
