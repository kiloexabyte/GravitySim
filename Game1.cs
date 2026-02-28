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
    private readonly List<Body> _bodies = [];
    private const float TimeStep = 0.01f;
    private const int SubSteps = 4;
    private Texture2D _pixel;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // Set window size in pixels
        _graphics.PreferredBackBufferWidth = 1920;  // width in pixels
        _graphics.PreferredBackBufferHeight = 1080;  // height in pixels
        _graphics.ApplyChanges();
        
        _bodies.Add(new Body(
            position: Vector2.Zero,
            velocity: Vector2.Zero,
            mass: 1000f,
            radius: 0.2f
        ));

        _bodies.Add(new Body(
            position: new Vector2(5, 0),
            velocity: new Vector2(0, MathF.Sqrt(Physics.G * 1000f / 5f)),
            mass: 1f,
            radius: 0.05f
        ));
        
        _bodies.Add(new Body(
            position: new Vector2(3, 0),
            velocity: new Vector2(0, MathF.Sqrt(Physics.G * 1000f / 5f)),
            mass: 1f,
            radius: 0.05f
        ));

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

        const float dt = TimeStep / SubSteps;

        for (var i = 0; i < SubSteps; i++)
            Physics.Step(_bodies, dt);

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

        const float zoom = 80f;

        foreach (var body in _bodies)
        {
            var screenPos = body.Position * zoom + screenCenter;
            var size = (2f + MathF.Log(body.Mass + 1f) * 2f) / 64f;

            _spriteBatch.Draw(
                _pixel,
                screenPos,
                null,
                Color.White,
                0f,
                new Vector2(0.5f, 0.5f),
                size,
                SpriteEffects.None,
                0f
            );
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
