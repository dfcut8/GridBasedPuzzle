using System.Collections.Generic;
using System.Linq;

using Godot;

using GridBasedPuzzle.Core;
using GridBasedPuzzle.Resources.Buildings;

namespace GridBasedPuzzle.Scenes.Components;

public partial class BuildingComponent : Node2D
{
    [Export(PropertyHint.File, "*.tres")]
    private string buildingResourcePath;
    [Export] private BuildingAnimatorComponent animatorComponent;

    public BuildingResource BuildingResource;

    public bool IsDestroing { get; private set; } = false;
    public bool IsDisabled { get; private set; } = false;

    private HashSet<Vector2I> occupiedTiles = [];

    public static IEnumerable<BuildingComponent> GetValidBuildingComponents(Node node)
    {
        return node.GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>()
            .Where(b => !b.IsDestroing);
    }

    public static IEnumerable<BuildingComponent> GetDangerBuildingComponents(Node node)
    {
        return GetValidBuildingComponents(node).Where(bc => bc.BuildingResource.IsDangerBuilding);
    }

    public override void _Ready()
    {
        if (buildingResourcePath is not null)
        {
            BuildingResource = GD.Load<BuildingResource>(buildingResourcePath);
        }
        AddToGroup(nameof(BuildingComponent));
        Callable.From(Initialize).CallDeferred();
    }

    public void Disable()
    {
        if (IsDisabled) return;
        IsDisabled = true;
        GlobalEvents.BuildingDisabled?.Invoke(this);
    }

    public void Enable()
    {
        if (!IsDisabled) return;
        IsDisabled = false;
        GlobalEvents.BuildingEnabled?.Invoke(this);
    }

    public Vector2I GetRootGridCellPosition()
    {
        var gridPosition = (GlobalPosition / 64).Floor();
        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
    }


    /// <summary>
    /// Returns a snapshot of the grid cells occupied by this building.
    /// </summary>
    /// <returns>
    /// A new <see cref="HashSet{Vector2I}"/> containing the positions occupied by this building.
    /// The returned set is a copy (snapshot); modifying it will not affect the component's internal state.
    /// </returns>
    public HashSet<Vector2I> GetOccupiedSellPositions()
    {
        // Return a copy to avoid exposing internal collection to callers.
        return [.. occupiedTiles];
    }

    public bool IsTileInBuildingArea(Vector2I pos)
    {
        return occupiedTiles.Contains(pos);
    }

    public Rect2I GetTileArea() => new(GetRootGridCellPosition(), BuildingResource.Dimensions);

    public void DestroyBuilding()
    {
        IsDestroing = true;
        if (BuildingResource.IsDeletable)
        {
            if (animatorComponent is null)
            {
                Owner.QueueFree();
                return;
            }
            else
            {
                animatorComponent.PlayDestroyAnimation();
                animatorComponent.DestroyAnimationFinished += () =>
                {
                    Owner.QueueFree();
                };
            }
            GlobalEvents.BuildingDestroyed?.Invoke(this);
        }
    }

    private void CalculateOccupiedSellPositions()
    {
        var gridPosition = GetRootGridCellPosition();
        for (int x = gridPosition.X; x < gridPosition.X + BuildingResource.Dimensions.X; x++)
        {
            for (int y = gridPosition.Y; y < gridPosition.Y + BuildingResource.Dimensions.Y; y++)
            {
                occupiedTiles.Add(new Vector2I(x, y));
            }
        }
    }

    private void Initialize()
    {
        CalculateOccupiedSellPositions();
        GlobalEvents.BuildingPlaced?.Invoke(this);
    }

}
