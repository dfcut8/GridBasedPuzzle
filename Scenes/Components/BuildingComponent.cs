using Godot;

using GridBasedPuzzle.Core;
using GridBasedPuzzle.Resources.Buildings;

namespace GridBasedPuzzle.Components;

public partial class BuildingComponent : Node2D
{
    [Export(PropertyHint.File, "*.tres")]
    private string buildingResourcePath;

    public BuildingResource BuildingResource;

    public override void _Ready()
    {
        if (buildingResourcePath is not null)
        {
            BuildingResource = GD.Load<BuildingResource>(buildingResourcePath);
        }
        AddToGroup(nameof(BuildingComponent));
        Callable.From(() => GlobalEvents.BuildingPlaced?.Invoke(this)).CallDeferred();
    }

    public Vector2I GetRootGridCellPosition()
    {
        var gridPosition = (GlobalPosition / 64).Floor();
        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
    }
}
