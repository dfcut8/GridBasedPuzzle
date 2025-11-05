using Godot;
using GridBasedPuzzle.Resources.Levels;

namespace GridBasedPuzzle.Autoloads;
public partial class LevelManager : Node
{
    [Export] private LevelResource[] levelResouces;

    private int currentLevelIndex = 0;

    public static LevelManager Instance { get; private set; }

    public LevelManager()
    {
        levelResouces = [];
        Instance = this;
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelResouces.Length)
        {
            GD.PrintErr($"Level index {levelIndex} is out of bounds.");
            return;
        }
        currentLevelIndex = levelIndex;
        var levelScene = levelResouces[levelIndex].LevelScenePath;
        if (levelScene == null)
        {
            GD.PrintErr($"Level scene at index {levelIndex} is null.");
            return;
        }
        GetTree().ChangeSceneToFile(levelScene);
    }

    public void LoadNextLevel()
    {
        LoadLevel(++currentLevelIndex);
    }
}
