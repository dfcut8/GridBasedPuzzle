using Godot;
using GridBasedPuzzle.Resources.Buildings;

namespace GridBasedPuzzle.UserInterface;

public partial class BuildingSection : PanelContainer
{
    [Export] private Label label;
    [Export] private Button button;

    public void Initialize(BuildingResource resource)
    {
        label.Text = resource.DisplayName.ToUpper();
        button.Text = $"SELECT ({resource.ResourceCost})";
    }
}
