using System.Collections.Generic;

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

    public List<Vector2I> GetOccupiedSellPositions()
    {
        var result = new List<Vector2I>();
        var gridPosition = GetRootGridCellPosition();
        for (int x = gridPosition.X; x < gridPosition.X + BuildingResource.Dimensions.X; x++)
        {
            for (int y = gridPosition.Y; y < gridPosition.Y + BuildingResource.Dimensions.Y; y++)
            {
                result.Add(new Vector2I(x, y));
            }
        }
        return result;
    }

    public void DestroyBuilding()
    {
        GlobalEvents.BuildingDestroyed?.Invoke(this);
        Owner.QueueFree();
    }
}
