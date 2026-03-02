using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = System.Numerics.Vector2;

namespace GravitySim;

public struct InputResult
{
    public bool ShouldExit;
    public bool ShouldReset;
}

public class InputHandler
{
    private MouseState _previousMouse;
    private KeyboardState _previousKeyboard;
    private bool _isDragging;
    private Vector2 _dragStart;
    private float _spawnMass = 1f;
    private string _massInput = "1";

    public bool IsDragging => _isDragging;
    public Vector2 DragStart => _dragStart;
    public float SpawnMass => _spawnMass;
    public string MassInput => _massInput;

    public InputResult Update(Viewport viewport, List<Body> bodies, float zoom, float velocityScale,
        Rectangle resetButton)
    {
        var result = new InputResult();
        var keyboard = Keyboard.GetState();

        if (keyboard.IsKeyDown(Keys.Escape))
            result.ShouldExit = true;

        if (keyboard.IsKeyDown(Keys.R))
            result.ShouldReset = true;

        for (var k = Keys.D0; k <= Keys.D9; k++)
        {
            if (keyboard.IsKeyDown(k) && !_previousKeyboard.IsKeyDown(k))
                _massInput += (char)('0' + (k - Keys.D0));
        }
        for (var k = Keys.NumPad0; k <= Keys.NumPad9; k++)
        {
            if (keyboard.IsKeyDown(k) && !_previousKeyboard.IsKeyDown(k))
                _massInput += (char)('0' + (k - Keys.NumPad0));
        }
        if (keyboard.IsKeyDown(Keys.OemPeriod) && !_previousKeyboard.IsKeyDown(Keys.OemPeriod)
            && !_massInput.Contains('.'))
            _massInput += '.';
        if (keyboard.IsKeyDown(Keys.Decimal) && !_previousKeyboard.IsKeyDown(Keys.Decimal)
            && !_massInput.Contains('.'))
            _massInput += '.';
        if (keyboard.IsKeyDown(Keys.Back) && !_previousKeyboard.IsKeyDown(Keys.Back) && _massInput.Length > 0)
            _massInput = _massInput[..^1];

        if (float.TryParse(_massInput, out var parsed) && parsed > 0)
            _spawnMass = parsed;

        _previousKeyboard = keyboard;

        var mouse = Mouse.GetState();
        var screenCenter = new Vector2(viewport.Width / 2f, viewport.Height / 2f);

        var clickedReset = false;
        if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released
            && resetButton.Contains(mouse.X, mouse.Y))
        {
            result.ShouldReset = true;
            clickedReset = true;
        }

        if (!clickedReset
            && mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released
            && mouse.X >= 0 && mouse.X < viewport.Width
            && mouse.Y >= 0 && mouse.Y < viewport.Height)
        {
            _isDragging = true;
            _dragStart = new Vector2(mouse.X, mouse.Y);
        }

        if (mouse.LeftButton == ButtonState.Released && _previousMouse.LeftButton == ButtonState.Pressed && _isDragging)
        {
            _isDragging = false;
            var dragEnd = new Vector2(mouse.X, mouse.Y);
            var worldPos = (_dragStart - screenCenter) / zoom;
            var velocity = (_dragStart - dragEnd) / zoom * velocityScale;
            bodies.Add(new Body(worldPos, velocity, _spawnMass, 0.05f));
        }

        _previousMouse = mouse;
        return result;
    }
}
