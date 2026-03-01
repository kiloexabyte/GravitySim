using System.Collections.Generic;
using System.Numerics;

namespace GravitySim;

public class Body
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Acceleration;

    public float Mass;
    public float Radius;

    public const int MaxTrailLength = 120;
    public readonly Queue<Vector2> Trail = new();

    public Body(Vector2 position, Vector2 velocity, float mass, float radius)
    {
        Position = position;
        Velocity = velocity;
        Mass = mass;
        Radius = radius;
        Acceleration = Vector2.Zero;
    }

    public void RecordTrail()
    {
        Trail.Enqueue(Position);
        if (Trail.Count > MaxTrailLength)
            Trail.Dequeue();
    }
}
