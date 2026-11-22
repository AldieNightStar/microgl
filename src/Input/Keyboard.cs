using System.Numerics;
using SDL3;

namespace microgl;

public static class Keyboard
{
    private static readonly HashSet<SDL.Keycode> _pressedKeys = new();

    public static event Action<SDL.Keycode>? OnDown;
    public static event Action<SDL.Keycode>? OnUp;

    public static bool IsCtrl => IsKeyDown(SDL.Keycode.LCtrl) || IsKeyDown(SDL.Keycode.RCtrl);
    public static bool IsShift => IsKeyDown(SDL.Keycode.LShift) || IsKeyDown(SDL.Keycode.RShift);
    public static bool IsAlt => IsKeyDown(SDL.Keycode.LAlt) || IsKeyDown(SDL.Keycode.RAlt);

    public static bool IsKeyDown(SDL.Keycode key) => _pressedKeys.Contains(key);
    public static bool IsKeyUp(SDL.Keycode key) => !_pressedKeys.Contains(key);

    public static Vector2 GetVectorFromKeys(DirectionKeys keys)
    {
        var direction = new Vector2(_getAxis(keys.Left, keys.Right), _getAxis(keys.Up, keys.Down));
        return direction == Vector2.Zero ? direction : Vector2.Normalize(direction);
    }

    internal static bool IsKeyboardEvent(ref SDL.Event ev)
    {
        var type = ev.Type;
        return type is (uint)SDL.EventType.KeyDown or (uint)SDL.EventType.KeyUp;
    }

    internal static void ProcessEvent(ref SDL.Event ev)
    {
        var key = ev.Key.Key;

        if (ev.Type == (uint)SDL.EventType.KeyDown) _press(key, ev.Key.Repeat);
        else _release(key);
    }

    internal static void ReleaseAll()
    {
        foreach (var key in _pressedKeys.ToArray()) _release(key);
    }

    private static void _press(SDL.Keycode key, bool repeated)
    {
        if (repeated) return;
        _pressedKeys.Add(key);
        OnDown?.Invoke(key);
    }

    private static void _release(SDL.Keycode key)
    {
        _pressedKeys.Remove(key);
        OnUp?.Invoke(key);
    }

    private static float _getAxis(SDL.Keycode negative, SDL.Keycode positive) =>
        (IsKeyDown(positive) ? 1f : 0f) - (IsKeyDown(negative) ? 1f : 0f);
}
