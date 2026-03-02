using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = System.Numerics.Vector2;

namespace GravitySim;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly List<Body> _bodies =
    [
        new(Vector2.Zero, Vector2.Zero, 1000f, 0.2f),
        new(new Vector2(3f, 0), new Vector2(0, MathF.Sqrt(Physics.G * 1000f / 3f)), 0.5f, 0.05f),
        new(new Vector2(0, 6f), new Vector2(-MathF.Sqrt(Physics.G * 1000f / 6f), 0), 1f, 0.05f),
        new(new Vector2(0.2f, 6f), new Vector2(-MathF.Sqrt(Physics.G * 1000f / 6f), MathF.Sqrt(Physics.G * 1f / 0.2f)), 0.03f, 0.02f),
        new(new Vector2(-10f, 0), new Vector2(0, -MathF.Sqrt(Physics.G * 1000f / 10f)), 1.5f, 0.05f),
        new(new Vector2(-10f, 0.25f), new Vector2(-MathF.Sqrt(Physics.G * 1.5f / 0.25f), -MathF.Sqrt(Physics.G * 1000f / 10f)), 0.04f, 0.02f),
        new(new Vector2(0, -15f), new Vector2(MathF.Sqrt(Physics.G * 1000f / 15f), 0), 0.8f, 0.05f),
        new(new Vector2(21f, 0), new Vector2(0, MathF.Sqrt(Physics.G * 1000f / 21f)), 10f, 0.05f),
        new(new Vector2(21.4f, 0), new Vector2(0, MathF.Sqrt(Physics.G * 1000f / 21f) + MathF.Sqrt(Physics.G * 10f / 0.4f)), 0.05f, 0.02f),
        new(new Vector2(-28f, 0), new Vector2(0, -MathF.Sqrt(Physics.G * 1000f / 28f)), 5f, 0.05f),
        new(new Vector2(-28f, 0.3f), new Vector2(-MathF.Sqrt(Physics.G * 5f / 0.3f), -MathF.Sqrt(Physics.G * 1000f / 28f)), 0.04f, 0.02f),
        new(new Vector2(0, 36f), new Vector2(-MathF.Sqrt(Physics.G * 1000f / 36f), 0), 3f, 0.05f),
        new(new Vector2(0.3f, 36f), new Vector2(-MathF.Sqrt(Physics.G * 1000f / 36f), MathF.Sqrt(Physics.G * 3f / 0.3f)), 0.05f, 0.02f),
        new(new Vector2(-45f, 0), new Vector2(0, -MathF.Sqrt(Physics.G * 1000f / 45f)), 0.3f, 0.03f),
        new(new Vector2(55f, 0), new Vector2(0, MathF.Sqrt(Physics.G * 1000f / 55f)), 8f, 0.05f),
        new(new Vector2(55f, 0.35f), new Vector2(-MathF.Sqrt(Physics.G * 8f / 0.35f), MathF.Sqrt(Physics.G * 1000f / 55f)), 0.05f, 0.02f),
        new(new Vector2(0, -66f), new Vector2(MathF.Sqrt(Physics.G * 1000f / 66f), 0), 0.6f, 0.03f),
    ];
    private const float TimeStep = 0.01f;
    private const int SubSteps = 4;
    private const float VelocityScale = 1.5f;
    private readonly Camera _camera = new();
    private static readonly Rectangle ResetButton = new(10, 10, 80, 32);

    private readonly InputHandler _inputHandler = new();
    private readonly Renderer _renderer = new();

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        Window.AllowUserResizing = true;
        _graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _renderer.LoadContent(GraphicsDevice, Content);
    }

    protected override void Update(GameTime gameTime)
    {
        var result = _inputHandler.Update(GraphicsDevice.Viewport, _bodies, _camera, VelocityScale, ResetButton, IsActive);

        if (result.ShouldExit)
            Exit();
        if (result.ShouldReset)
        {
            _bodies.Clear();
            _camera.Position = Vector2.Zero;
            _camera.Zoom = 80f;
        }

        const float dt = TimeStep / SubSteps;
        for (var i = 0; i < SubSteps; i++)
            Physics.Step(_bodies, dt);

        foreach (var body in _bodies)
            body.RecordTrail();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _renderer.Draw(GraphicsDevice, _bodies, _inputHandler, _camera, ResetButton, VelocityScale);
        base.Draw(gameTime);
    }
}
