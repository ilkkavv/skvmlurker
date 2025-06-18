using Godot;
using System;

public partial class MouseLook : Node3D
{
    [Export] public float Sensitivity = 0.002f;
    [Export] public NodePath CameraPath;

    private Camera3D camera;
    private float pitch = 0.0f;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        camera = GetNode<Camera3D>(CameraPath);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            RotateY(-mouseMotion.Relative.X * Sensitivity);

            pitch -= mouseMotion.Relative.Y * Sensitivity;
            pitch = Mathf.Clamp(pitch, -Mathf.Pi / 2, Mathf.Pi / 2); // limit up/down

            camera.Rotation = new Vector3(pitch, 0, 0);
        }

        // Escape to release mouse
        if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }
}