using System;

using Godot;

using GridBasedPuzzle.Resources.Buildings;

namespace GridBasedPuzzle.UserInterface;

public partial class Ui : MarginContainer
{
    [Export] private BuildingResource[] buildingResources;

    public Action PlaceTowerButtonPressed;
    public Action PlaceVillageButtonPressed;

    private HBoxContainer hBoxContainer;

    public override void _Ready()
    {
        hBoxContainer = GetNode<HBoxContainer>("%HBoxContainer");
        CreateBuildingButtons();
    }

    private void CreateBuildingButtons()
    {
        foreach (var br in buildingResources)
        {
            var buildingButton = new Button();
            buildingButton.Text = "PLACE BUILDING";
            hBoxContainer.AddChild(buildingButton);
        }
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
