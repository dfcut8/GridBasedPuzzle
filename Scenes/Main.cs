using Godot;

using GridBasedPuzzle.Managers;

namespace GridBasedPuzzle.Core;

public partial class Main : Node
{
    [Export] private GridManager gridManager;
    [Export] private BuildingManager buildingManager;
    [Export] private Node2D goldMine;

    public override void _Ready()
    {
        gridManager.GridStateUpdated += () =>
        {
            var goldMineTilePosition = gridManager.ConvertWorldPositionToTilePosition(goldMine.GlobalPosition);
            if (gridManager.IsTilePositionBuildable(goldMineTilePosition))
            {
                GD.Print("win");
            }
        };
    }
}
