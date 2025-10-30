using Godot;
using GridBasedPuzzle.Resources.Buildings;
using System;

namespace GridBasedPuzzle.UserInterface;

public partial class BuildingSection : PanelContainer
{
    private Label title;
    private Label cost;
    private Label description;
    private Button button;

    public Action Pressed;

    public override void _Ready()
    {
        title = GetNode<Label>("%Title");
        cost = GetNode<Label>("%Cost");
        description = GetNode<Label>("%Description");
        button.Pressed += () => Pressed?.Invoke();
    }

    public void Initialize(BuildingResource resource)
    {
        title.Text = resource.DisplayName;
        cost.Text = resource.ResourceCost.ToString();
        description.Text = resource.Description;
    }
}
