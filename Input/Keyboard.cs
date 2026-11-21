using System.Numerics;
using SDL3;

namespace microgl;

public static class Keyboard
{
    private static readonly HashSet<SDL.Keycode> _heldKeys = new();
    private static readonly HashSet<SDL.Keycode> _pressedKeys = new();
    private static readonly HashSet<SDL.Keycode> _releasedKeys = new();

    public static event Action<SDL.Keycode>? OnDown;
    public static event Action<SDL.Keycode>? OnUp;

    public static bool IsCtrl => IsKeyDown(SDL.Keycode.LCtrl) || IsKeyDown(SDL.Keycode.RCtrl);
    public static bool IsShift => IsKeyDown(SDL.Keycode.LShift) || IsKeyDown(SDL.Keycode.RShift);
    public static bool IsAlt => IsKeyDown(SDL.Keycode.LAlt) || IsKeyDown(SDL.Keycode.RAlt);

    public static bool IsKeyDown(SDL.Keycode key) => _heldKeys.Contains(key);
    public static bool IsKeyPressed(SDL.Keycode key) => _pressedKeys.Contains(key);
    public static bool IsKeyReleased(SDL.Keycode key) => _releasedKeys.Contains(key);

    public static Vector2 GetVectorFromKeys(DirectionKeys keys) =>
        GetVectorFromKeys(keys.Up, keys.Left, keys.Down, keys.Right);

    public static Vector2 GetVectorFromKeys(SDL.Keycode up, SDL.Keycode left, SDL.Keycode down, SDL.Keycode right)
    {
        var direction = new Vector2(_getAxis(left, right), _getAxis(up, down));
        return direction == Vector2.Zero ? direction : Vector2.Normalize(direction);
    }

    internal static void BeginFrame()
    {
        _pressedKeys.Clear();
        _releasedKeys.Clear();
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

    private static void _press(SDL.Keycode key, bool isRepeat)
    {
        if (!isRepeat)
        {
            _heldKeys.Add(key);
            _pressedKeys.Add(key);
        }

        OnDown?.Invoke(key);
    }

    private static void _release(SDL.Keycode key)
    {
        _heldKeys.Remove(key);
        _releasedKeys.Add(key);
        OnUp?.Invoke(key);
    }

    private static float _getAxis(SDL.Keycode negative, SDL.Keycode positive) =>
        (IsKeyDown(positive) ? 1f : 0f) - (IsKeyDown(negative) ? 1f : 0f);
}
