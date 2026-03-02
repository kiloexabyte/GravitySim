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
    private bool _isPanning;

    public bool IsDragging => _isDragging;
    public Vector2 DragStart => _dragStart;
    public float SpawnMass => _spawnMass;
    public string MassInput => _massInput;

    public InputResult Update(Viewport viewport, List<Body> bodies, Camera camera, float velocityScale,
        Rectangle resetButton, bool isActive)
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

        if (isActive)
        {
            var screenCenter = new Vector2(viewport.Width / 2f, viewport.Height / 2f);

            // Scroll wheel zoom toward cursor
            var scrollDelta = mouse.ScrollWheelValue - _previousMouse.ScrollWheelValue;
            if (scrollDelta != 0)
            {
                var cursorScreen = new Vector2(mouse.X, mouse.Y);
                var worldBeforeZoom = camera.ScreenToWorld(cursorScreen, screenCenter);

                var factor = scrollDelta > 0 ? 1.1f : 1f / 1.1f;
                camera.Zoom = Math.Clamp(camera.Zoom * factor, 10f, 1000f);

                var worldAfterZoom = camera.ScreenToWorld(cursorScreen, screenCenter);
                camera.Position -= worldAfterZoom - worldBeforeZoom;
            }

            // Middle mouse pan
            if (mouse.MiddleButton == ButtonState.Pressed && _previousMouse.MiddleButton == ButtonState.Released)
                _isPanning = true;

            if (_isPanning && mouse.MiddleButton == ButtonState.Pressed)
            {
                var mouseDelta = new Vector2(mouse.X - _previousMouse.X, mouse.Y - _previousMouse.Y);
                camera.Position -= mouseDelta / camera.Zoom;
            }

            if (mouse.MiddleButton == ButtonState.Released)
                _isPanning = false;

            // Reset button click
            var clickedReset = false;
            if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released
                && resetButton.Contains(mouse.X, mouse.Y))
            {
                result.ShouldReset = true;
                clickedReset = true;
            }

            // Left-click drag to spawn
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
                var worldPos = camera.ScreenToWorld(_dragStart, screenCenter);
                var dragEndWorld = camera.ScreenToWorld(dragEnd, screenCenter);
                var velocity = (worldPos - dragEndWorld) * velocityScale;
                bodies.Add(new Body(worldPos, velocity, _spawnMass, 0.05f));
            }
        }
        else
        {
            _isDragging = false;
            _isPanning = false;
        }

        _previousMouse = mouse;
        return result;
    }
}
