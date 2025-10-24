using System;

using Godot;

using GridBasedPuzzle.Resources.Buildings;

namespace GridBasedPuzzle.UserInterface;

public partial class Ui : CanvasLayer
{
    [Export] private BuildingResource[] buildingResources;

    public Action<BuildingResource> BuildingResourceSelected;

    private HBoxContainer hBoxContainer;
    private Label usedLabel;
    private Label availableLabel;
    private Label currentLabel;

    public override void _Ready()
    {
        hBoxContainer = GetNode<HBoxContainer>("%HBoxContainer");
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
            var buildingButton = new Button();
            buildingButton.Text = $"PLACE {br.DisplayName.ToUpper()} ({br.ResourceCost})";
            hBoxContainer.AddChild(buildingButton);

            buildingButton.Pressed += () =>
            {
                BuildingResourceSelected?.Invoke(br);
            };
        }
    }
}
