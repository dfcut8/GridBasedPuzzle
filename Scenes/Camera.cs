using Godot;

using GridBasedPuzzle.Scenes.Core;

namespace GridBasedPuzzle;

public partial class Camera : Camera2D
{
    [Export] private int cameraSpeed = 800;

    private readonly string INPUT_CAMERA_PAN_LEFT = "camera_pan_left";
    private readonly string INPUT_CAMERA_PAN_RIGHT = "camera_pan_right";
    private readonly string INPUT_CAMERA_PAN_UP = "camera_pan_up";
    private readonly string INPUT_CAMERA_PAN_DOWN = "camera_pan_down";

    public override void _Process(double delta)
    {
        GlobalPosition = GetScreenCenterPosition();

        var movement = Input.GetVector(
            INPUT_CAMERA_PAN_LEFT,
            INPUT_CAMERA_PAN_RIGHT,
            INPUT_CAMERA_PAN_UP,
            INPUT_CAMERA_PAN_DOWN).Normalized();

        GlobalPosition += movement * cameraSpeed * (float)delta;
    }

    public void SetBoundariesRect(Rect2I rect)
    {
        LimitLeft = rect.Position.X * GlobalConstants.TILE_SIZE_PIXELS;
        LimitRight = rect.End.X * GlobalConstants.TILE_SIZE_PIXELS;
        LimitTop = rect.Position.Y * GlobalConstants.TILE_SIZE_PIXELS;
        LimitBottom = rect.End.Y * GlobalConstants.TILE_SIZE_PIXELS;
    }

    public void SetCameraPosition(Vector2 pos)
    {
        GlobalPosition = pos;
    }
}
