using Godot;

namespace GridBasedPuzzle.Managers;

public partial class GlobalInputManager : Node
{
    public const string GLOBAL_INPUT_STAGE_RESET = "global_stage_reset";
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(GLOBAL_INPUT_STAGE_RESET))
        {
            GetTree().ReloadCurrentScene();
        }
    }
}
