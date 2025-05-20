using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Catalyst.Systems;

/// <summary>
/// Handles inputs for MonoGame in the same style as Godot's Input class, based on named actions.
/// </summary>
/// <remarks>
/// Reference: https://docs.godotengine.org/en/stable/classes/class_input.html
/// </remarks>
public static class InputSystem
{
    private static readonly Dictionary<string, HashSet<Keys>> Actions = [];
    private static KeyboardState _previousKeyboardState = Keyboard.GetState();
    private static KeyboardState _currentKeyboardState = Keyboard.GetState();
    private static MouseState _previousMouseState = Mouse.GetState();
    private static MouseState _currentMouseState = Mouse.GetState();

    /// <summary>
    /// Defines mouse buttons for use with InputSystem.
    /// </summary>
    public enum MouseButton
    {
        Left,
        Right,
        Middle,
        XButton1,
        XButton2
    }

    /// <summary>
    /// Updates the internal keyboard and mouse states.
    /// Must be called once per frame to enable edge detection (just pressed/released).
    /// </summary>
    public static void Update()
    {
        _previousKeyboardState = _currentKeyboardState;
        _currentKeyboardState = Keyboard.GetState();
        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();
    }

    /// <summary>
    /// Creates an action that will listen to the specified keys.
    /// Overwrites any existing action with the same name.
    /// </summary>
    public static void CreateAction(string actionName, IEnumerable<Keys> actionKeys)
    {
        Actions[actionName] = new HashSet<Keys>(actionKeys);
    }

    /// <summary>
    /// Appends a key to an existing action.
    /// Fails if the action does not exist.
    /// </summary>
    /// <returns>True if the key was added; false if the action does not exist.</returns>
    public static bool AddKeyToAction(string actionName, Keys actionKey)
    {
        if (!Actions.TryGetValue(actionName, out var keys))
            return false;
        keys.Add(actionKey);
        return true;
    }

    /// <summary>
    /// Retrieves the keys associated with an action.
    /// </summary>
    /// <returns>The keys if the action exists; otherwise, null.</returns>
    public static HashSet<Keys>? GetKeysFromAction(string actionName)
    {
        return Actions.GetValueOrDefault(actionName);
    }

    /// <summary>
    /// Deletes an existing action.
    /// </summary>
    /// <returns>True if the action was removed; false if it did not exist.</returns>
    public static bool DeleteAction(string actionName)
    {
        return Actions.Remove(actionName);
    }

    /// <summary>
    /// Checks whether an action with the specified name exists.
    /// </summary>
    /// <returns>True if the action exists; otherwise, false.</returns>
    public static bool HasAction(string actionName)
    {
        return Actions.ContainsKey(actionName);
    }

    /// <summary>
    /// Checks whether any keys associated with the action are currently pressed.
    /// </summary>
    /// <returns>True if at least one key is currently pressed; otherwise, false.</returns>
    public static bool IsActionPressed(string actionName)
    {
        var keys = GetKeysFromAction(actionName);
        return keys != null && keys.Any(key => _currentKeyboardState.IsKeyDown(key));
    }

    /// <summary>
    /// Checks whether any keys associated with the action were just pressed this frame.
    /// Requires Update() to be called once per frame.
    /// </summary>
    /// <returns>True if at least one key was just pressed; otherwise, false.</returns>
    public static bool IsActionJustPressed(string actionName)
    {
        var keys = GetKeysFromAction(actionName);
        return keys != null && keys.Any(key => _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key));
    }

    /// <summary>
    /// Checks whether any keys associated with the action were just released this frame.
    /// Requires Update() to be called once per frame.
    /// </summary>
    /// <returns>True if at least one key was just released; otherwise, false.</returns>
    public static bool IsActionJustReleased(string actionName)
    {
        var keys = GetKeysFromAction(actionName);
        return keys != null && keys.Any(key => _currentKeyboardState.IsKeyUp(key) && _previousKeyboardState.IsKeyDown(key));
    }

    // New Mouse Input Methods

    /// <summary>
    /// Checks if a specific mouse button is currently pressed.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>True if the button is pressed; otherwise, false.</returns>
    public static bool IsMouseButtonPressed(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => _currentMouseState.LeftButton == ButtonState.Pressed,
            MouseButton.Right => _currentMouseState.RightButton == ButtonState.Pressed,
            MouseButton.Middle => _currentMouseState.MiddleButton == ButtonState.Pressed,
            MouseButton.XButton1 => _currentMouseState.XButton1 == ButtonState.Pressed,
            MouseButton.XButton2 => _currentMouseState.XButton2 == ButtonState.Pressed,
            _ => false,
        };
    }

    /// <summary>
    /// Checks if a specific mouse button was just pressed this frame.
    /// Requires Update() to be called once per frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>True if the button was just pressed; otherwise, false.</returns>
    public static bool IsMouseButtonJustPressed(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => _currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released,
            MouseButton.Right => _currentMouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released,
            MouseButton.Middle => _currentMouseState.MiddleButton == ButtonState.Pressed && _previousMouseState.MiddleButton == ButtonState.Released,
            MouseButton.XButton1 => _currentMouseState.XButton1 == ButtonState.Pressed && _previousMouseState.XButton1 == ButtonState.Released,
            MouseButton.XButton2 => _currentMouseState.XButton2 == ButtonState.Pressed && _previousMouseState.XButton2 == ButtonState.Released,
            _ => false,
        };
    }

    /// <summary>
    /// Checks if a specific mouse button was just released this frame.
    /// Requires Update() to be called once per frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns>True if the button was just released; otherwise, false.</returns>
    public static bool IsMouseButtonJustReleased(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => _currentMouseState.LeftButton == ButtonState.Released && _previousMouseState.LeftButton == ButtonState.Pressed,
            MouseButton.Right => _currentMouseState.RightButton == ButtonState.Released && _previousMouseState.RightButton == ButtonState.Pressed,
            MouseButton.Middle => _currentMouseState.MiddleButton == ButtonState.Released && _previousMouseState.MiddleButton == ButtonState.Pressed,
            MouseButton.XButton1 => _currentMouseState.XButton1 == ButtonState.Released && _previousMouseState.XButton1 == ButtonState.Pressed,
            MouseButton.XButton2 => _currentMouseState.XButton2 == ButtonState.Released && _previousMouseState.XButton2 == ButtonState.Pressed,
            _ => false,
        };
    }

    /// <summary>
    /// Gets the current mouse position in screen coordinates.
    /// </summary>
    /// <returns>A Vector2 representing the mouse's X and Y coordinates.</returns>
    public static Vector2 GetMousePosition()
    {
        return new Vector2(_currentMouseState.X, _currentMouseState.Y);
    }

    /// <summary>
    /// Gets the change in mouse position since the last frame.
    /// </summary>
    /// <returns>A Vector2 representing the delta in mouse X and Y coordinates.</returns>
    public static Vector2 GetMouseDelta()
    {
        return new Vector2(_currentMouseState.X - _previousMouseState.X, _currentMouseState.Y - _previousMouseState.Y);
    }

    /// <summary>
    /// Gets the change in mouse scroll wheel value since the last frame.
    /// </summary>
    /// <returns>The difference in scroll wheel value.</returns>
    public static int GetMouseScrollDelta()
    {
        return _currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
    }
}
