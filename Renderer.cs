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
    }

    public void Draw(GraphicsDevice graphicsDevice, List<Body> bodies, InputHandler input,
        float zoom, Rectangle resetButton)
    {
        graphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        Vector2 screenCenter = new(
            graphicsDevice.Viewport.Width / 2f,
            graphicsDevice.Viewport.Height / 2f
        );

        foreach (var body in bodies)
        {
            var trailArray = body.Trail.ToArray();
            for (var i = 1; i < trailArray.Length; i++)
            {
                var alpha = (float)i / trailArray.Length * 0.5f;
                var from = trailArray[i - 1] * zoom + screenCenter;
                var to = trailArray[i] * zoom + screenCenter;
                DrawLine(from, to, Color.White * alpha);
            }

            var screenPos = body.Position * zoom + screenCenter;
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
