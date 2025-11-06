using Godot;
using GridBasedPuzzle.Managers;
using GridBasedPuzzle.Resources.Levels;
using GridBasedPuzzle.Scenes.Ui;
using BuildingManager = GridBasedPuzzle.Scenes.Managers.BuildingManager;
using LevelCompleteScreen = GridBasedPuzzle.Scenes.Ui.LevelCompleteScreen;

namespace GridBasedPuzzle.Scenes.Levels;

public partial class BaseLevel : Node
{
    [Export] private GridManager gridManager;
    [Export] private BuildingManager buildingManager;
    [Export] private Node2D baseBuilding;
    [Export] private Scenes.GoldMine goldMine;
    [Export] private Scenes.Camera camera;
    [Export] private TileMapLayer baseLayer;
    [Export] private PackedScene levelCompleteScreenScene;
    [Export] private Hud ui;
    [Export] private LevelResource levelResource;

    public override void _Ready()
    {
        camera.SetBoundariesRect(baseLayer.GetUsedRect());
        camera.SetCameraPosition(baseBuilding.GlobalPosition);
        if (levelResource == null)
        {
            GD.PushError("LevelResource is not assigned in the inspector.");
            return;
        }
        buildingManager.SetStartingResourceCount(levelResource.StartingResourceCount);

        gridManager.GridStateUpdated += () =>
        {
            var goldMineTilePosition = gridManager.ConvertWorldPositionToTilePosition(goldMine.GlobalPosition);
            if (gridManager.IsTilePositionInAnyBuildingRadius(goldMineTilePosition))
            {
                ShowCompleteScreen();
                goldMine.SetActive();
            }
        };
    }

    private void ShowCompleteScreen()
    {
        var levelCompleteScreenInstance = levelCompleteScreenScene.Instantiate<LevelCompleteScreen>();
        AddChild(levelCompleteScreenInstance);
        ui.HideUi();
    }
}
