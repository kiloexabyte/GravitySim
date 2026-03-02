using Vector2 = System.Numerics.Vector2;

namespace GravitySim;

public class Camera
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Zoom { get; set; } = 80f;

    public Vector2 WorldToScreen(Vector2 worldPos, Vector2 screenCenter) =>
        (worldPos - Position) * Zoom + screenCenter;

    public Vector2 ScreenToWorld(Vector2 screenPos, Vector2 screenCenter) =>
        (screenPos - screenCenter) / Zoom + Position;
}
