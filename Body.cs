using System.Collections.Generic;
using System.Numerics;

namespace GravitySim;

public class Body(Vector2 position, Vector2 velocity, float mass, float radius)
{
    public Vector2 Position = position;
    public Vector2 Velocity = velocity;
    public Vector2 Acceleration = Vector2.Zero;

    public float Mass = mass;
    public float Radius = radius;

    public const int MaxTrailLength = 120;
    public readonly Queue<Vector2> Trail = new();

    public void RecordTrail()
    {
        Trail.Enqueue(Position);
        if (Trail.Count > MaxTrailLength)
            Trail.Dequeue();
    }
}
