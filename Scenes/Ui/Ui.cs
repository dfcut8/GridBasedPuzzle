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
    private Label usedLabel;
    private Label availableLabel;
    private Label currentLabel;

    public override void _Ready()
    {
        buttonsContainer = GetNode<VBoxContainer>("%ButtonsContainer");
        usedLabel = GetNode<PanelContainer>("%Used").GetNode<Label>("Text");
        availableLabel = GetNode<PanelContainer>("%Available").GetNode<Label>("Text");
        currentLabel = GetNode<PanelContainer>("%Current").GetNode<Label>("Text");

        // Make sure UI is always starts visible
        Visible = true;

        CreateBuildingButtons();
    }

    public void UpdateResources(int used, int available, int current)
    {
        usedLabel.Text = used.ToString();
        availableLabel.Text = available.ToString();
        currentLabel.Text = current.ToString();
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
