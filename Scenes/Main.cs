using Godot;

using GridBasedPuzzle.Managers;
using GridBasedPuzzle.Resources.Buildings;

namespace GridBasedPuzzle.Core;

public partial class Main : Node
{
    [Export] private GridManager gridManager;
    [Export] private Node2D ySortRoot;
    private Sprite2D cursor;
    private Button placeTowerButton;
    private Button placeVillageButton;
    private Vector2I? hoveredGridCell;


    // Resources
    private BuildingResource towerResource;
    private BuildingResource villageResource;

    private BuildingResource toPlaceBuildingResource;

    public override void _Ready()
    {
        LoadBuildingResources();
        InitCursor();
        InitButtons();
    }

    private void InitButtons()
    {
        placeTowerButton = GetNode<Button>("%PlaceTowerButton");
        placeTowerButton.Pressed += () =>
        {
            toPlaceBuildingResource = towerResource;
            cursor.Visible = true;
            gridManager.HighlightBuildableTiles();
        };

        placeVillageButton = GetNode<Button>("%PlaceVillageButton");
        placeVillageButton.Pressed += () =>
        {
            toPlaceBuildingResource = villageResource;
            cursor.Visible = true;
            gridManager.HighlightBuildableTiles();
        };

        gridManager.ResourceTilesUpdated += count =>
        {
            GD.Print($"Resource Count: {count}");
        };
    }

    private void InitCursor()
    {
        cursor = GetNode<Sprite2D>("%Cursor");
        cursor.Visible = false;
    }

    private void LoadBuildingResources()
    {
        towerResource = GD.Load<BuildingResource>("res://Resources/Buildings/Tower.tres");
        villageResource = GD.Load<BuildingResource>("res://Resources/Buildings/Village.tres");
    }

    public override void _Process(double delta)
    {
        var gridPosition = gridManager.GetMouseGridCellPosition();
        cursor.GlobalPosition = gridPosition * 64;
        if (toPlaceBuildingResource is not null
            && cursor.Visible
            && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition))
        {
            hoveredGridCell = gridPosition;
            gridManager.ClearHighlightedTiles();
            gridManager.HighlightExpandedBuildableTiles(hoveredGridCell.Value, toPlaceBuildingResource.BuildableRadius);
            gridManager.HighlightResourceTiles(hoveredGridCell.Value, toPlaceBuildingResource.ResourceRadius);
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (hoveredGridCell.HasValue
            && e.IsActionPressed("mouse_click_left")
            && gridManager.IsTilePositionBuildable(hoveredGridCell.Value))
        {
            PlaceBuildingAtHoveredCellPosition();
            cursor.Visible = false;
        }
    }

    private void PlaceBuildingAtHoveredCellPosition()
    {
        if (!hoveredGridCell.HasValue) return;

        var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
        ySortRoot.AddChild(building);

        building.GlobalPosition = hoveredGridCell.Value * 64;

        hoveredGridCell = null;
        gridManager.ClearHighlightedTiles();
    }
}
