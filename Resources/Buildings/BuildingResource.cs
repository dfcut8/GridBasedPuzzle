using Godot;

namespace GridBasedPuzzle.Resources.Buildings;

[GlobalClass]
public partial class BuildingResource : Resource
{
    [Export] public string DisplayName { get; private set; }
    [Export] public Vector2I Dimensions { get; private set; } = Vector2I.One;
    [Export] public bool IsDeletable { get; private set; } = true;
    [Export] public int ResourceCost { get; private set; }
    [Export] public int BuildableRadius { get; private set; }
    [Export] public int ResourceRadius { get; private set; }
    [Export] public PackedScene BuildingScene { get; private set; }
    [Export] public PackedScene BuildingSpriteScene { get; private set; }
}
