using Godot;
using GridBasedPuzzle.Autoloads;

namespace GridBasedPuzzle.UserInterface;

public partial class LevelSelection : MarginContainer
{
    [Export] private PackedScene levelSelectionElement;

    override public void _Ready()
    {
        var levels = LevelManager.GetAllLevels();
        var grid = GetNode<GridContainer>("%GridContainer");
        for (int i = 0; i < levels.Length; i++)
        {
            var levelElement = levelSelectionElement.Instantiate<LevelSelectionElement>();
            grid.AddChild(levelElement);
            levelElement.Setup(levels[i], i);
        }
    }
}
