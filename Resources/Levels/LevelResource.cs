using Godot;

namespace GridBasedPuzzle.Resources.Levels;

[GlobalClass]
public partial class LevelResource : Resource
{
    [Export] public string Id { get; private set; }
    [Export] public int StartingResourceCount { get; private set; }
    [Export(PropertyHint.File, "Level*.tscn")] public string LevelScenePath { get; private set; }
}
