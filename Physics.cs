using System;
using System.Collections.Generic;
using System.Numerics;

namespace GravitySim;

public static class Physics
{
    public const float G = 1.0f;
    public const float Softening = 0.1f;

    public static void Step(List<Body> bodies, float dt)
    {
        // Reset accelerations
        foreach (var b in bodies)
            b.Acceleration = Vector2.Zero;

        // Pairwise gravity
        for (var i = 0; i < bodies.Count; i++)
        {
            for (var j = i + 1; j < bodies.Count; j++)
            {
                var bi = bodies[i];
                var bj = bodies[j];

                var r = bj.Position - bi.Position;
                var distSq = r.LengthSquared() + Softening * Softening;
                var invDist = 1.0f / MathF.Sqrt(distSq);
                var invDist3 = invDist * invDist * invDist;

                var a = G * r * invDist3;

                bi.Acceleration += a * bj.Mass;
                bj.Acceleration -= a * bi.Mass;
            }
        }

        // Semi-implicit Euler
        foreach (var b in bodies)
        {
            b.Velocity += b.Acceleration * dt;
            b.Position += b.Velocity * dt;
        }
    }
}