using Godot;

using GridBasedPuzzle.Managers;

namespace GridBasedPuzzle.Core;

public partial class Main : Node
{
    [Export] private GridManager gridManager;
    [Export] private BuildingManager buildingManager;


    public override void _Ready()
    {

    }
}
