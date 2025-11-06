using System;
using Godot;
using GridBasedPuzzle.Resources.Buildings;
using GridBasedPuzzle.UserInterface;

namespace GridBasedPuzzle.Scenes.Ui;

public partial class Hud : CanvasLayer
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

        CreateBuildingButtons();
    }

    public void UpdateAvailableResources(int resourceCount)
    {
        availableResourceLabel.Text = resourceCount.ToString();
    }

    public void HideUi()
    {
        Visible = false;
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
