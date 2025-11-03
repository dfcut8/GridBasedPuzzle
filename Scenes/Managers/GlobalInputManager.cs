using Godot;
using GridBasedPuzzle.Autoloads;

namespace GridBasedPuzzle.Managers;

public partial class GlobalInputManager : Node
{
    public const string GLOBAL_INPUT_STAGE_RESET = "global_stage_reset";
    public const string GLOBAL_INPUT_STAGE_LOADNEXT = "global_stage_loadnext";
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(GLOBAL_INPUT_STAGE_RESET))
        {
            GetTree().ReloadCurrentScene();
        }

        if (Input.IsActionJustPressed(GLOBAL_INPUT_STAGE_LOADNEXT))
        {
            LevelManager.Instance.LoadNextLevel();
        }
    }
}
