using Godot;

public partial class Isometrics3 : Node2D
{
    [Export] private TileMapLayer tileMap;
    [Export] private PackedScene tileAnimationSprite;

    public override void _Ready()
    {
        var tiles = tileMap.GetUsedCells();
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        var tile = tiles[rng.RandiRange(0, tiles.Count)];
        GD.Print($"selected tile: {tile}");
        // var tileData = tileMap.GetCellTileData(tile);
        tileMap.SetCell(tile);
    }
}
