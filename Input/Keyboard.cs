using SDL3;

namespace microgl;

public class Keyboard
{
    public static event Action<SDL.Keycode> OnDown;
    public static event Action<SDL.Keycode> OnUp;

    public static bool IsCtrl { get; private set; }
    public static bool IsShift { get; private set; }
    public static bool IsAlt { get; private set; }

    internal static bool IsKeyboardEvent(ref SDL.Event ev)
    {
        var type = ev.Type;
        return type is (uint)SDL.EventType.KeyDown or (uint)SDL.EventType.KeyUp;
    }

    internal static void ProcessEvent(ref SDL.Event ev)
    {
        var type = (SDL.EventType)ev.Type;
        var isDown = type == SDL.EventType.KeyDown;
        var key = ev.Key.Key;

        if (isDown) OnDown?.Invoke(key);
        else OnUp?.Invoke(key);

        switch (key)
        {
            case SDL.Keycode.LCtrl or SDL.Keycode.RCtrl:
                IsCtrl = isDown;
                break;
            case SDL.Keycode.LShift or SDL.Keycode.RShift:
                IsShift = isDown;
                break;
            case SDL.Keycode.LAlt or SDL.Keycode.RAlt:
                IsAlt = isDown;
                break;
        }
    }
}