using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = System.Numerics.Vector2;

namespace GravitySim;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
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
    private Texture2D _pixel;
    private MouseState _previousMouse;
    private bool _isDragging;
    private Vector2 _dragStart;
    private const float Zoom = 80f;
    private const float VelocityScale = 0.1f;

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
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        const int diameter = 64;
        const float radius = diameter / 2f;
        _pixel = new Texture2D(GraphicsDevice, diameter, diameter);
        var data = new Color[diameter * diameter];
        for (var y = 0; y < diameter; y++)
        {
            for (var x = 0; x < diameter; x++)
            {
                var dx = x - radius + 0.5f;
                var dy = y - radius + 0.5f;
                data[y * diameter + x] = dx * dx + dy * dy <= radius * radius
                    ? Color.White
                    : Color.Transparent;
            }
        }
        _pixel.SetData(data);
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var mouse = Mouse.GetState();
        var screenCenter = new Vector2(
            GraphicsDevice.Viewport.Width / 2f,
            GraphicsDevice.Viewport.Height / 2f
        );

        if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released
            && mouse.X >= 0 && mouse.X < GraphicsDevice.Viewport.Width
            && mouse.Y >= 0 && mouse.Y < GraphicsDevice.Viewport.Height)
        {
            _isDragging = true;
            _dragStart = new Vector2(mouse.X, mouse.Y);
        }

        if (mouse.LeftButton == ButtonState.Released && _previousMouse.LeftButton == ButtonState.Pressed && _isDragging)
        {
            _isDragging = false;
            var dragEnd = new Vector2(mouse.X, mouse.Y);
            var worldPos = (_dragStart - screenCenter) / Zoom;
            var velocity = (_dragStart - dragEnd) / Zoom * VelocityScale;
            _bodies.Add(new Body(worldPos, velocity, 1f, 0.05f));
        }

        _previousMouse = mouse;

        const float dt = TimeStep / SubSteps;

        for (var i = 0; i < SubSteps; i++)
            Physics.Step(_bodies, dt);

        foreach (var body in _bodies)
            body.RecordTrail();

        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        Vector2 screenCenter = new(
            GraphicsDevice.Viewport.Width / 2f,
            GraphicsDevice.Viewport.Height / 2f
        );

        foreach (var body in _bodies)
        {
            var trailArray = body.Trail.ToArray();
            for (var i = 1; i < trailArray.Length; i++)
            {
                var alpha = (float)i / trailArray.Length * 0.5f;
                var from = trailArray[i - 1] * Zoom + screenCenter;
                var to = trailArray[i] * Zoom + screenCenter;
                DrawLine(from, to, Color.White * alpha);
            }

            var screenPos = body.Position * Zoom + screenCenter;
            var size = (2f + MathF.Log(body.Mass + 1f) * 2f) / 64f;

            _spriteBatch.Draw(
                _pixel,
                screenPos,
                null,
                Color.White,
                0f,
                new Vector2(32f, 32f),
                size,
                SpriteEffects.None,
                0f
            );
        }

        if (_isDragging)
        {
            var mouse = Mouse.GetState();
            var from = _dragStart;
            var to = new Vector2(mouse.X, mouse.Y);
            DrawLine(from, to, Color.Gray);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawLine(Vector2 from, Vector2 to, Color color)
    {
        var diff = to - from;
        var length = diff.Length();
        if (length < 1f) return;
        var angle = MathF.Atan2(diff.Y, diff.X);
        _spriteBatch.Draw(
            _pixel,
            from,
            null,
            color,
            angle,
            new Vector2(0, 32f),
            new Vector2(length / 64f, 1f / 64f),
            SpriteEffects.None,
            0f
        );
    }
}
