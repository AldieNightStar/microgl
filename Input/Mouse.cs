using System.Numerics;
using SDL3;

namespace microgl;

public static class Mouse
{
    public const uint LEFT = 1;
    public const uint MIDDLE = 2;
    public const uint RIGHT = 3;

    private const int _buttonCount = 32;

    private static readonly uint[] _mouseEvents = [
        (uint) SDL.EventType.MouseMotion,
        (uint) SDL.EventType.MouseButtonDown,
        (uint) SDL.EventType.MouseButtonUp,
        (uint) SDL.EventType.MouseWheel,
    ];

    public static event Action<Vector2>? OnMove;
    public static event Action<uint>? OnDown;
    public static event Action<uint>? OnUp;
    public static event Action<Vector2>? OnWheel;

    private static uint _heldMask;

    public static Vector2 Position { get; private set; }
    public static bool IsDown(uint button) => (_heldMask & (1u << (int)button)) != 0;

    internal static void ProcessEvent(Drawer drawer, ref SDL.Event ev)
    {
        var type = ev.Type;

        // Convert coordinates to window
        SDL.ConvertEventToRenderCoordinates(drawer.Renderer.GetNativePointer(), ref ev);

        if (type == (uint)SDL.EventType.MouseMotion)
        {
            var motion = ev.Motion;
            Position = new Vector2(motion.X, motion.Y);
            OnMove?.Invoke(Position);
        }
        else if (type == (uint)SDL.EventType.MouseButtonDown)
        {
            var buttons = ev.Button;
            Position = new Vector2(buttons.X, buttons.Y);
            _setHeld(buttons.Button);
            OnDown?.Invoke(buttons.Button);
        }
        else if (type == (uint)SDL.EventType.MouseButtonUp)
        {
            var buttons = ev.Button;
            Position = new Vector2(buttons.X, buttons.Y);
            _unsetHeld(buttons.Button);
            OnUp?.Invoke(buttons.Button);
        }
        else if (type == (uint)SDL.EventType.MouseWheel)
        {
            var wheel = ev.Wheel;
            float dir = wheel.Direction == SDL.MouseWheelDirection.Flipped ? -1f : 1f;
            OnWheel?.Invoke(new Vector2(wheel.X * dir, wheel.Y * dir));
        }
    }

    internal static void ReleaseAll()
    {
        for (uint button = 0; button < _buttonCount; button++)
        {
            if (!IsDown(button)) continue;
            _unsetHeld(button);
            OnUp?.Invoke(button);
        }
    }

    private static void _setHeld(uint button) => _heldMask |= 1u << (int)button;
    private static void _unsetHeld(uint button) => _heldMask &= ~(1u << (int)button);

    internal static bool IsMouseEvent(ref SDL.Event ev) => _mouseEvents.Contains(ev.Type);
}