using Godot;
using GridBasedPuzzle.Autoloads;
using System;

namespace GridBasedPuzzle.UserInterface;

public partial class LevelSelection : MarginContainer
{
    public Action BackButtonPressed;

    [Export] private PackedScene levelSelectionElement;

    private Button backButton;

    override public void _Ready()
    {
        backButton = GetNode<Button>("%BackButton");
        backButton.Pressed += () => BackButtonPressed?.Invoke();

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
