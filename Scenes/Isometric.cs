using System.Collections.Generic;

using Godot;

public partial class Isometric : Node2D
{
    [Export] private TileMapLayer tileMap;
    [Export] private Texture2D tilesetTexture; // set this to your tileset image used by the TileMap
    [Export] private Vector2I cellSize = new Vector2I(64, 64);
    [Export] private float fallDistance = 600f;  // how far above screen the tile starts
    [Export] private float baseDuration = 6f;  // base duration for fall
    [Export] private float staggerPerTile = 0.3f; // delay offset per tile to create wave

    public override void _Ready()
    {
        if (tilesetTexture == null)
        {
            GD.PrintErr("Isometric: `tilesetTexture` is not assigned. Falling sprites will not render.");
            return;
        }

        var tiles = tileMap.GetUsedCells();
        if (tiles.Count == 0) return;

        var rng = new RandomNumberGenerator();
        rng.Randomize();

        // Convert to a list and sort so delays are deterministic and never zero
        var tilesList = new List<Vector2I>(tiles);
        tilesList.Sort((a, b) => a.Y != b.Y ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

        for (int i = 0; i < tilesList.Count; i++)
        {
            var cell = tilesList[i];
            // get tile data (we need tile source origin inside the atlas)
            var tileData = tileMap.GetCellTileData(cell);

            // compute target position in world using the tilemap's mapping (keeps transforms and offsets correct)
            // prefer MapToWorld + ToGlobal to match where the tilemap actually draws the cell
            // compute naive world position (TileMapLayer may not expose MapToWorld)
            var localPos = new Vector2(cell.X * cellSize.X, cell.Y * cellSize.Y);
            var targetPos = tileMap.ToGlobal(localPos);

            // create an AtlasTexture cropping the tileset using the tile's TextureOrigin
            var atlas = new AtlasTexture();
            atlas.Atlas = tilesetTexture;
            atlas.Region = new Rect2((Vector2)tileData.TextureOrigin, (Vector2)cellSize);

            // create sprite overlay
            var sprite = new Sprite2D();
            sprite.Texture = atlas;
            sprite.Visible = true;
            // ensure overlay is drawn above tilemap
            sprite.ZIndex = 1000;
            // start somewhere above — keep x aligned with target; y above screen by fallDistance
            sprite.GlobalPosition = new Vector2(targetPos.X, -fallDistance);
            AddChild(sprite); // add to scene so the sprite is visible

            // duration and stagger per tile (randomized a bit)
            var duration = baseDuration + (float)rng.RandfRange(0.0f, 0.25f);
            // use index-based delay so no tile gets zero delay unless staggerPerTile == 0
            var delay = i * staggerPerTile;
            // small random jitter to avoid exact simultaneity
            delay += (float)rng.RandfRange(0.0f, 0.02f);

            // Use a short Timer to stagger the start of each sprite's tween
            var startTimer = new Timer();
            // enforce a small minimum so very early tiles are still staggered visibly
            startTimer.WaitTime = Mathf.Max(0.01f, delay);
            startTimer.OneShot = true;
            AddChild(startTimer);
            startTimer.Timeout += () =>
            {
                GD.Print($"Isometric: starting fall for cell {cell} -> {targetPos} after {startTimer.WaitTime:F3}s");
                var tween = CreateTween();
                tween.TweenProperty(sprite, "global_position", targetPos, duration).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
                tween.TweenCallback(Callable.From(() => sprite.QueueFree()));
                startTimer.QueueFree();
            };
            startTimer.Start();
        }
    }
}