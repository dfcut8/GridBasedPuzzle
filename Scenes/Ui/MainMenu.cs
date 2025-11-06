using Godot;
using System;

namespace GridBasedPuzzle.UserInterface;

public partial class MainMenu : Node
{
    private Button playButton;
    private Button quitButton;
    private Button optionsButton;
    private MarginContainer mainMenu;
    private Scenes.Ui.LevelSelection levelSelection;

    public override void _Ready()
    {
        playButton = GetNode<Button>("%PlayButton");
        quitButton = GetNode<Button>("%QuitButton");
        optionsButton = GetNode<Button>("%OptionsButton");

        mainMenu = GetNode<MarginContainer>("%MainMenu");
        mainMenu.Visible = true;

        levelSelection = GetNode<Scenes.Ui.LevelSelection>("%LevelSelection");
        levelSelection.Visible = false;
        levelSelection.BackButtonPressed += ToggleLevelSelection;

        playButton.Pressed += ToggleLevelSelection;
        quitButton.Pressed += OnQuitButtonPressed;
        optionsButton.Pressed += OnOptionsButtonPressed;
    }

    private void ToggleLevelSelection()
    {
        mainMenu.Visible = !mainMenu.Visible;
        levelSelection.Visible = !levelSelection.Visible;
    }

    private void OnOptionsButtonPressed()
    {
        throw new NotImplementedException();
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
