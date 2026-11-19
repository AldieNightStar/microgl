using System.Numerics;
using SDL3;

namespace microgl;

public class Mouse
{
    public const uint LEFT = 1;
    public const uint MIDDLE = 2;
    public const uint RIGHT = 3;

    private static uint[] MOUSE_EVENTS = [
        (uint) SDL.EventType.MouseMotion,
        (uint) SDL.EventType.MouseButtonDown,
        (uint) SDL.EventType.MouseButtonUp,
        (uint) SDL.EventType.MouseWheel,
    ];

    internal static Vector2 LastPosition;

    public static event Action<Vector2> OnMove;
    public static event Action<uint> OnDown;
    public static event Action<uint> OnUp;

    public static Vector2 Position => LastPosition;

    internal static void ProcessEvent(Drawer drawer, ref SDL.Event ev)
    {
        var type = ev.Type;

        // Convert coordinates to window
        SDL.ConvertEventToRenderCoordinates(drawer.Renderer.GetNativePointer(), ref ev);

        if (type == (uint)SDL.EventType.MouseMotion)
        {
            var motion = ev.Motion;
            LastPosition = new Vector2(motion.X, motion.Y);
            OnMove?.Invoke(LastPosition);
        }

        else if (type == (uint)SDL.EventType.MouseButtonDown)
        {
            var buttons = ev.Button;
            LastPosition = new Vector2(buttons.X, buttons.Y);
            OnDown?.Invoke(buttons.Button);
        }

        else if (type == (uint)SDL.EventType.MouseButtonUp)
        {
            var buttons = ev.Button;
            LastPosition = new Vector2(buttons.X, buttons.Y);
            OnUp?.Invoke(buttons.Button);
        }
    }

    internal static bool IsMouseEvent(ref SDL.Event ev) => MOUSE_EVENTS.Contains(ev.Type);
}