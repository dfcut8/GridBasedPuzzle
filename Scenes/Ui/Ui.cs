using System;

using Godot;

namespace GridBasedPuzzle.UserInterface;

public partial class Ui : MarginContainer
{
    private Button placeTowerButton;
    private Button placeVillageButton;

    public Action PlaceTowerButtonPressed;
    public Action PlaceVillageButtonPressed;

    public override void _Ready()
    {
        placeTowerButton = GetNode<Button>("%PlaceTowerButton");
        placeTowerButton.Pressed += OnPlaceTowerButtonPressed;

        placeVillageButton = GetNode<Button>("%PlaceVillageButton");
        placeVillageButton.Pressed += OnPlaceVillageButtonPressed;
    }

    private void OnPlaceVillageButtonPressed()
    {
        PlaceTowerButtonPressed?.Invoke();
    }

    private void OnPlaceTowerButtonPressed()
    {
        PlaceVillageButtonPressed?.Invoke();
    }
}
