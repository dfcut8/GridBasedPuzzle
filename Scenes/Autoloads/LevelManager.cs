using Godot;

namespace GridBasedPuzzle.Autoloads;
public partial class LevelManager : Node
{
    [Export] private PackedScene[] levelScenes;

    private int currentLevelIndex = 0;

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
        currentLevelIndex = levelIndex;
        var levelScene = levelScenes[levelIndex];
        if (levelScene == null)
        {
            GD.PrintErr($"Level scene at index {levelIndex} is null.");
            return;
        }
        GetTree().ChangeSceneToPacked(levelScene);
    }

    public void LoadNextLevel()
    {
        LoadLevel(++currentLevelIndex);
    }
}
