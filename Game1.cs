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
        new(new Vector2(1.5f, 0), new Vector2(0, MathF.Sqrt(Physics.G * 1000f / 1.5f)), 0.5f, 0.05f),
        new(new Vector2(0, 2.5f), new Vector2(-MathF.Sqrt(Physics.G * 1000f / 2.5f), 0), 1f, 0.05f),
        new(new Vector2(-4f, 0), new Vector2(0, -MathF.Sqrt(Physics.G * 1000f / 4f)), 1.5f, 0.05f),
        new(new Vector2(0, -5.5f), new Vector2(MathF.Sqrt(Physics.G * 1000f / 5.5f), 0), 0.8f, 0.05f),
        new(new Vector2(8f, 0), new Vector2(0, MathF.Sqrt(Physics.G * 1000f / 8f)), 10f, 0.05f),
        new(new Vector2(-10.5f, 0), new Vector2(0, -MathF.Sqrt(Physics.G * 1000f / 10.5f)), 5f, 0.05f),
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
