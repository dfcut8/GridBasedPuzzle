using Godot;

namespace GridBasedPuzzle;

public partial class Camera : Camera2D
{
    [Export] private int cameraSpeed = 600;

    private readonly string INPUT_CAMERA_PAN_LEFT = "camera_pan_left";
    private readonly string INPUT_CAMERA_PAN_RIGHT = "camera_pan_right";
    private readonly string INPUT_CAMERA_PAN_UP = "camera_pan_up";
    private readonly string INPUT_CAMERA_PAN_DOWN = "camera_pan_down";

    public override void _Process(double delta)
    {
        var movement = Input.GetVector(
            INPUT_CAMERA_PAN_LEFT,
            INPUT_CAMERA_PAN_RIGHT,
            INPUT_CAMERA_PAN_UP,
            INPUT_CAMERA_PAN_DOWN).Normalized();

        GlobalPosition += movement * cameraSpeed * (float)delta;
    }
}
