using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

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
    private static KeyboardState _previousState = Keyboard.GetState();
    private static KeyboardState _currentState = Keyboard.GetState();

    /// <summary>
    /// Updates the internal keyboard states.
    /// Must be called once per frame to enable edge detection (just pressed/released).
    /// </summary>
    public static void Update()
    {
        _previousState = _currentState;
        _currentState = Keyboard.GetState();
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
        return keys != null && keys.Any(key => _currentState.IsKeyDown(key));
    }

    /// <summary>
    /// Checks whether any keys associated with the action were just pressed this frame.
    /// Requires Update() to be called once per frame.
    /// </summary>
    /// <returns>True if at least one key was just pressed; otherwise, false.</returns>
    public static bool IsActionJustPressed(string actionName)
    {
        var keys = GetKeysFromAction(actionName);
        return keys != null && keys.Any(key => _currentState.IsKeyDown(key) && _previousState.IsKeyUp(key));
    }

    /// <summary>
    /// Checks whether any keys associated with the action were just released this frame.
    /// Requires Update() to be called once per frame.
    /// </summary>
    /// <returns>True if at least one key was just released; otherwise, false.</returns>
    public static bool IsActionJustReleased(string actionName)
    {
        var keys = GetKeysFromAction(actionName);
        return keys != null && keys.Any(key => _currentState.IsKeyUp(key) && _previousState.IsKeyDown(key));
    }
}
