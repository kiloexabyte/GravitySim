using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = System.Numerics.Vector2;

namespace GravitySim;

public class Renderer
{
    private SpriteBatch _spriteBatch;
    private Texture2D _pixel;
    private Texture2D _rectPixel;
    private SpriteFont _font;

    private const int StarCount = 300;
    private const int StarLayers = 3;
    private static readonly float[] LayerParallax = [0.02f, 0.05f, 0.1f];
    private static readonly float[] LayerBrightness = [0.15f, 0.3f, 0.5f];
    private static readonly float[] LayerSize = [1f, 1.5f, 2f];
    private Vector2[][] _starPositions;
    private int _starFieldWidth;
    private int _starFieldHeight;

    public void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
    {
        _spriteBatch = new SpriteBatch(graphicsDevice);

        const int diameter = 64;
        const float radius = diameter / 2f;
        _pixel = new Texture2D(graphicsDevice, diameter, diameter);
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

        _rectPixel = new Texture2D(graphicsDevice, 1, 1);
        _rectPixel.SetData(new[] { Color.White });

        _font = content.Load<SpriteFont>("DefaultFont");

        _starFieldWidth = graphicsDevice.Viewport.Width;
        _starFieldHeight = graphicsDevice.Viewport.Height;
        var rng = new Random(42);
        _starPositions = new Vector2[StarLayers][];
        for (var layer = 0; layer < StarLayers; layer++)
        {
            _starPositions[layer] = new Vector2[StarCount];
            for (var i = 0; i < StarCount; i++)
                _starPositions[layer][i] = new Vector2(
                    (float)(rng.NextDouble() * _starFieldWidth),
                    (float)(rng.NextDouble() * _starFieldHeight));
        }
    }

    public void Draw(GraphicsDevice graphicsDevice, List<Body> bodies, InputHandler input,
        Camera camera, Rectangle resetButton)
    {
        graphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        Vector2 screenCenter = new(
            graphicsDevice.Viewport.Width / 2f,
            graphicsDevice.Viewport.Height / 2f
        );

        // Parallax starfield
        for (var layer = 0; layer < StarLayers; layer++)
        {
            var offsetX = camera.Position.X * camera.Zoom * LayerParallax[layer];
            var offsetY = camera.Position.Y * camera.Zoom * LayerParallax[layer];
            var color = Color.White * LayerBrightness[layer];
            var size = LayerSize[layer];

            foreach (var star in _starPositions[layer])
            {
                var sx = ((star.X - offsetX) % _starFieldWidth + _starFieldWidth) % _starFieldWidth;
                var sy = ((star.Y - offsetY) % _starFieldHeight + _starFieldHeight) % _starFieldHeight;
                _spriteBatch.Draw(_rectPixel, new Rectangle((int)sx, (int)sy, (int)size, (int)size), color);
            }
        }

        foreach (var body in bodies)
        {
            var trailArray = body.Trail.ToArray();
            for (var i = 1; i < trailArray.Length; i++)
            {
                var alpha = (float)i / trailArray.Length * 0.5f;
                var from = camera.WorldToScreen(trailArray[i - 1], screenCenter);
                var to = camera.WorldToScreen(trailArray[i], screenCenter);
                DrawLine(from, to, Color.White * alpha);
            }

            var screenPos = camera.WorldToScreen(body.Position, screenCenter);
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

        if (input.IsDragging)
        {
            var mouse = Mouse.GetState();
            var from = input.DragStart;
            var to = new Vector2(mouse.X, mouse.Y);
            DrawLine(from, to, Color.Gray);

            var dir = from - to;
            var len = dir.Length();
            if (len > 10f)
            {
                var norm = dir / len;
                var perp = new Vector2(-norm.Y, norm.X);
                const float headLen = 12f;
                const float headWidth = 6f;
                var tip = from;
                DrawLine(tip, tip - norm * headLen + perp * headWidth, Color.Gray);
                DrawLine(tip, tip - norm * headLen - perp * headWidth, Color.Gray);
            }
        }

        _spriteBatch.Draw(_rectPixel, resetButton, Color.DarkSlateGray);
        var textSize = _font.MeasureString("Reset");
        var textPos = new Vector2(
            resetButton.X + (resetButton.Width - textSize.X) / 2f,
            resetButton.Y + (resetButton.Height - textSize.Y) / 2f
        );
        _spriteBatch.DrawString(_font, "Reset", textPos, Color.White);

        var massLabel = $"Mass: {input.MassInput}";
        var massTextSize = _font.MeasureString(massLabel);
        var massRect = new Rectangle(10, resetButton.Bottom + 6,
            (int)massTextSize.X + 16, (int)massTextSize.Y + 8);
        _spriteBatch.Draw(_rectPixel, massRect, Color.DarkSlateGray);
        var massTextPos = new Vector2(
            massRect.X + (massRect.Width - massTextSize.X) / 2f,
            massRect.Y + (massRect.Height - massTextSize.Y) / 2f
        );
        _spriteBatch.DrawString(_font, massLabel, massTextPos, Color.White);

        var bodyLabel = $"Bodies: {bodies.Count}";
        var bodyTextSize = _font.MeasureString(bodyLabel);
        var bodyRect = new Rectangle(10, massRect.Bottom + 6,
            (int)bodyTextSize.X + 16, (int)bodyTextSize.Y + 8);
        _spriteBatch.Draw(_rectPixel, bodyRect, Color.DarkSlateGray);
        var bodyTextPos = new Vector2(
            bodyRect.X + (bodyRect.Width - bodyTextSize.X) / 2f,
            bodyRect.Y + (bodyRect.Height - bodyTextSize.Y) / 2f
        );
        _spriteBatch.DrawString(_font, bodyLabel, bodyTextPos, Color.White);

        _spriteBatch.End();
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
