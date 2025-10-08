using Godot;

namespace GridBasedPuzzle.Components;

public partial class BuildingComponent : Node2D
{
    [Export] public int BuildableRadius { get; private set; }

    public override void _Ready()
    {
        AddToGroup(nameof(BuildingComponent));
    }
}
