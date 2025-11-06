using Godot;
using GridBasedPuzzle.Autoloads;
using GridBasedPuzzle.Resources.Levels;

namespace GridBasedPuzzle.Scenes.Ui;

public partial class LevelSelectionElement : PanelContainer
{
    private Label levelLabel;
    private Label resourceCountLabel;
    private Button selectButton;

    public override void _Ready()
    {
        levelLabel = GetNode<Label>("%LevelNumberLabel");
        resourceCountLabel = GetNode<Label>("%ResourceCountLabel");
        selectButton = GetNode<Button>("%SelectButton");
    }

    public void Setup(LevelResource levelResouce, int levelNumber)
    {
        levelLabel.Text = $"Level {levelNumber + 1}";
        resourceCountLabel.Text = levelResouce.StartingResourceCount.ToString();
        selectButton.Pressed += () =>
        {
            LevelManager.Instance.LoadLevel(levelNumber);
        };
    }
}
