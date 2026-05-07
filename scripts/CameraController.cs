using Godot;

namespace Rouge;

public partial class CameraController : Camera2D
{
    [Export] public Node2D Target;
    [Export] public float SmoothSpeed = 6.0f;

    // Level bounds
    public float LeftBound = -10000;
    public float RightBound = 10000;
    public float TopBound = -10000;
    public float BottomBound = 10000;

    public override void _Ready()
    {
        if (Target == null)
            Target = GetParent()?.GetNodeOrNull<CharacterBody2D>("Player");
    }

    public override void _Process(double delta)
    {
        if (Target == null) return;

        Vector2 targetPos = Target.GlobalPosition;

        // Smooth follow
        GlobalPosition = GlobalPosition.Lerp(targetPos, (float)delta * SmoothSpeed);

        // Clamp to bounds
        float halfW = GetViewportRect().Size.X / 2 * Zoom.X;
        float halfH = GetViewportRect().Size.Y / 2 * Zoom.Y;
        GlobalPosition = new Vector2(
            Mathf.Clamp(GlobalPosition.X, LeftBound + halfW, RightBound - halfW),
            Mathf.Clamp(GlobalPosition.Y, TopBound + halfH, BottomBound - halfH)
        );
    }

    public void SetBounds(float left, float right, float top, float bottom)
    {
        LeftBound = left;
        RightBound = right;
        TopBound = top;
        BottomBound = bottom;
    }
}
