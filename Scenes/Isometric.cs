using Godot;

public partial class Isometric : Node2D
{
    [Export] private TileMapLayer tileMap;

    public override void _Ready()
    {
        var tiles = tileMap.GetUsedCells();
        if (tiles.Count > 0)
        {
            var randomTilePos = tiles[(int)(GD.Randi() % (uint)tiles.Count)];
            var tileData = tileMap.GetCellTileData(randomTilePos);

            GD.Print($"Random tile: {randomTilePos}");
            GD.Print($"Tile data: {tileData}");
            // Move the random tile 40 pixels up
            tileData.TextureOrigin += new Vector2I(0, -40);
        }
    }
}
