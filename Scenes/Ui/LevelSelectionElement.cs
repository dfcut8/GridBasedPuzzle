using Godot;
using GridBasedPuzzle.Resources.Levels;

namespace GridBasedPuzzle.UserInterface;

public partial class LevelSelectionElement : PanelContainer
{
    private Label levelLabel;
    private Label resourceCountLabel;

    public override void _Ready()
    {
        levelLabel = GetNode<Label>("%LevelNumberLabel");
        resourceCountLabel = GetNode<Label>("%ResourceCountLabel");
    }

    public void Setup(LevelResource levelResouce, int levelNumber)
    {
        levelLabel.Text = $"Level {levelNumber}";
        resourceCountLabel.Text = levelResouce.StartingResourceCount.ToString();
    }
}
