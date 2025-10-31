using Godot;

namespace GridBasedPuzzle.Autoloads;
public partial class LevelManager : Node
{
    [Export] private PackedScene[] levelScenes;

    public static LevelManager Instance { get; private set; }

    public LevelManager()
    {
        levelScenes = [];
        Instance = this;
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelScenes.Length)
        {
            GD.PrintErr($"Level index {levelIndex} is out of bounds.");
            return;
        }
        var levelScene = levelScenes[levelIndex];
        if (levelScene == null)
        {
            GD.PrintErr($"Level scene at index {levelIndex} is null.");
            return;
        }
        GetTree().ChangeSceneToPacked(levelScene);
    }
}
