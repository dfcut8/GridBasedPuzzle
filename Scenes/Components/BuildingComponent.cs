using Godot;

using GridBasedPuzzle.Globals;

namespace GridBasedPuzzle.Components;

public partial class BuildingComponent : Node2D
{
    [Export] public int BuildableRadius { get; private set; }

    public override void _Ready()
    {
        AddToGroup(nameof(BuildingComponent));
        GlobalEvents.BuildingPlaced?.Invoke(this);
    }

    public Vector2I GetRootGridCellPosition()
    {
        var gridPosition = (GlobalPosition / 64).Floor();
        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
    }
}
