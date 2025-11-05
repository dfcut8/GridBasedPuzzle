using Godot;
using System;

namespace GridBasedPuzzle.UserInterface;

public partial class MainMenu : Node
{
    private Button playButton;
    private Button quitButton;
    private Button optionsButton;
    private MarginContainer mainMenu;
    private LevelSelection levelSelection;

    public override void _Ready()
    {
        playButton = GetNode<Button>("%PlayButton");
        quitButton = GetNode<Button>("%QuitButton");
        optionsButton = GetNode<Button>("%OptionsButton");

        mainMenu = GetNode<MarginContainer>("%MainMenu");
        mainMenu.Visible = true;

        levelSelection = GetNode<LevelSelection>("%LevelSelection");
        levelSelection.Visible = false;

        playButton.Pressed += OnPlayButtonPressed;
        quitButton.Pressed += OnQuitButtonPressed;
        optionsButton.Pressed += OnOptionsButtonPressed;
    }

    private void OnOptionsButtonPressed()
    {
        throw new NotImplementedException();
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    private void OnPlayButtonPressed()
    {
        mainMenu.Visible = false;
        levelSelection.Visible = true;
    }
}
