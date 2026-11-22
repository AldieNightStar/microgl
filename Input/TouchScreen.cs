using System.Numerics;
using SDL3;

namespace microgl;

public static class TouchScreen
{
    private static readonly uint[] _touchEvents = [
        (uint) SDL.EventType.FingerDown,
        (uint) SDL.EventType.FingerMotion,
        (uint) SDL.EventType.FingerUp,
        (uint) SDL.EventType.FingerCanceled,
    ];

    private static readonly Dictionary<ulong, Vector2> _fingers = new();

    public static event Action<ulong, Vector2>? OnDown;
    public static event Action<ulong, Vector2>? OnMove;
    public static event Action<ulong, Vector2>? OnUp;

    public static int ActiveCount => _fingers.Count;

    public static bool IsDown(ulong fingerId) => _fingers.ContainsKey(fingerId);

    public static bool TryGetPosition(ulong fingerId, out Vector2 position) => _fingers.TryGetValue(fingerId, out position);

    public static void CollectPositions(List<Vector2> buffer)
    {
        buffer.Clear();
        foreach (var position in _fingers.Values) buffer.Add(position);
    }

    internal static void ProcessEvent(Drawer drawer, ref SDL.Event ev)
    {
        SDL.ConvertEventToRenderCoordinates(drawer.Renderer.GetNativePointer(), ref ev);

        var type = ev.Type;
        var finger = ev.TFinger;
        var position = new Vector2(finger.X, finger.Y);

        if (type == (uint)SDL.EventType.FingerDown) _press(finger.FingerID, position);
        else if (type == (uint)SDL.EventType.FingerMotion) _move(finger.FingerID, position);
        else _release(finger.FingerID, position);
    }

    internal static void ReleaseAll()
    {
        foreach (var pair in _fingers.ToArray()) OnUp?.Invoke(pair.Key, pair.Value);
        _fingers.Clear();
    }

    internal static bool IsTouchEvent(ref SDL.Event ev) => Array.IndexOf(_touchEvents, ev.Type) >= 0;

    private static void _press(ulong fingerId, Vector2 position)
    {
        _fingers[fingerId] = position;
        OnDown?.Invoke(fingerId, position);
    }

    private static void _move(ulong fingerId, Vector2 position)
    {
        if (!_fingers.ContainsKey(fingerId)) return;
        _fingers[fingerId] = position;
        OnMove?.Invoke(fingerId, position);
    }

    private static void _release(ulong fingerId, Vector2 position)
    {
        if (!_fingers.Remove(fingerId)) return;
        OnUp?.Invoke(fingerId, position);
    }
}
