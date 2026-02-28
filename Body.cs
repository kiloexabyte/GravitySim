using System.Numerics;

namespace GravitySim;

public class Body
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Acceleration;

    public float Mass;
    public float Radius;

    public Body(Vector2 position, Vector2 velocity, float mass, float radius)
    {
        Position = position;
        Velocity = velocity;
        Mass = mass;
        Radius = radius;
        Acceleration = Vector2.Zero;
    }
}
