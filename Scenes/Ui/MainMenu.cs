using Godot;
using GridBasedPuzzle.Autoloads;
using System;

namespace GridBasedPuzzle.UserInterface;

public partial class MainMenu : Node
{
    private Button playButton;
    private Button quitButton;
    private Button optionsButton;

    public override void _Ready()
    {
        playButton = GetNode<Button>("%PlayButton");
        quitButton = GetNode<Button>("%QuitButton");
        optionsButton = GetNode<Button>("%OptionsButton");

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
        LevelManager.Instance.LoadLevel(0);
    }
}
