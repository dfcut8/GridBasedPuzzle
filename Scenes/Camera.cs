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
    private readonly string INPUT_CAMERA_ZOOM_OUT = "camera_zoom_out";
    private readonly string INPUT_CAMERA_ZOOM_IN = "camera_zoom_in";
    private float maxZoomIn = 3.0f;
    private float maxZoomOut = 0.5f;
    private float zoomStep = 0.25f;
    private float currentZoom = 1.0f;

    public override void _Process(double delta)
    {
        GlobalPosition = GetScreenCenterPosition();

        var movement = Input.GetVector(
            INPUT_CAMERA_PAN_LEFT,
            INPUT_CAMERA_PAN_RIGHT,
            INPUT_CAMERA_PAN_UP,
            INPUT_CAMERA_PAN_DOWN).Normalized();

        GlobalPosition += movement * cameraSpeed * (float)delta;

        HandleZoomInput();
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

    private void HandleZoomInput()
    {
        if (Input.IsActionJustPressed(INPUT_CAMERA_ZOOM_IN))
        {
            if (currentZoom > maxZoomOut)
            {
                currentZoom -= zoomStep;
            }
        }
        else if (Input.IsActionJustPressed(INPUT_CAMERA_ZOOM_OUT))
        {
            if (currentZoom < maxZoomIn)
            {
                currentZoom += zoomStep;
            }
        }
        Zoom = new Vector2(currentZoom, currentZoom);
    }
}
