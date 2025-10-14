using System;

using Godot;

using GridBasedPuzzle.Resources.Buildings;

namespace GridBasedPuzzle.UserInterface;

public partial class Ui : MarginContainer
{
    [Export] private BuildingResource[] buildingResources;

    public Action<BuildingResource> BuildingResourceSelected;

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
            buildingButton.Text = $"PLACE {br.DisplayName.ToUpper()}";
            hBoxContainer.AddChild(buildingButton);

            buildingButton.Pressed += () =>
            {
                BuildingResourceSelected?.Invoke(br);
            };
        }
    }
}
