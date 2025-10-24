using Godot;

using GridBasedPuzzle.Managers;

namespace GridBasedPuzzle.Core;

public partial class BaseLevel : Node
{
    [Export] private GridManager gridManager;
    [Export] private BuildingManager buildingManager;
    [Export] private Node2D baseBuilding;
    [Export] private GoldMine goldMine;
    [Export] private Camera camera;
    [Export] private TileMapLayer baseLayer;

    public override void _Ready()
    {
        camera.SetBoundariesRect(baseLayer.GetUsedRect());
        camera.SetCameraPosition(baseBuilding.GlobalPosition);

        gridManager.GridStateUpdated += () =>
        {
            var goldMineTilePosition = gridManager.ConvertWorldPositionToTilePosition(goldMine.GlobalPosition);
            if (gridManager.IsTilePositionBuildable(goldMineTilePosition))
            {
                GD.Print("win");
                goldMine.SetActive();
            }
        };
    }
}
