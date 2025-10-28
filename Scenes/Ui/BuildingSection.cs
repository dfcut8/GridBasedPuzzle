using Godot;
using GridBasedPuzzle.Resources.Buildings;
using System;

namespace GridBasedPuzzle.UserInterface;

public partial class BuildingSection : PanelContainer
{
    [Export] private Label label;
    [Export] private Button button;

    public Action Pressed;

    public override void _Ready()
    {
        button.Pressed += () => Pressed?.Invoke();
    }

    public void Initialize(BuildingResource resource)
    {
        label.Text = resource.DisplayName.ToUpper();
        button.Text = $"SELECT ({resource.ResourceCost})";
    }
}
