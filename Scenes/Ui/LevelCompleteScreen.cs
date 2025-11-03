using Godot;
using GridBasedPuzzle.Autoloads;

namespace GridBasedPuzzle.UserInterface;

public partial class LevelCompleteScreen : CanvasLayer
{
    private Button nextlevelButton;
    public override void _Ready()
    {
        nextlevelButton = GetNode<Button>("%NextLevelButton");
        nextlevelButton.Pressed += OnNextLevelButtonPressed;
    }
    private void OnNextLevelButtonPressed()
    {
        LevelManager.Instance.LoadNextLevel();
        QueueFree();
    }
}
