using Godot;
using GridBasedPuzzle.Resources.Buildings;
using System;

namespace GridBasedPuzzle.UserInterface;

public partial class Ui : CanvasLayer
{
    [Export] private BuildingResource[] buildingResources;
    [Export] private PackedScene buildingSectionScene;

    public Action<BuildingResource> BuildingResourceSelected;

    private VBoxContainer buttonsContainer;
    private Label availableResourceLabel;
    private Label currentLabel;

    public override void _Ready()
    {
        buttonsContainer = GetNode<VBoxContainer>("%ButtonsContainer");
        availableResourceLabel = GetNode<Label>("%Resource");

        // Make sure UI is always starts visible
        Visible = true;

        CreateBuildingButtons();
    }

    public void UpdateAvailableResources(int resourceCount)
    {
        availableResourceLabel.Text = resourceCount.ToString();
    }

    private void CreateBuildingButtons()
    {
        foreach (var br in buildingResources)
        {
            var buildingSectionInstance = buildingSectionScene.Instantiate<BuildingSection>();
            buttonsContainer.AddChild(buildingSectionInstance);
            buildingSectionInstance.Initialize(br);

            buildingSectionInstance.Pressed += () =>
            {
                BuildingResourceSelected?.Invoke(br);
            };
        }
    }
}
