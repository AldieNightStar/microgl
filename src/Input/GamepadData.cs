using System.Numerics;
using SDL3;

namespace microgl;

internal sealed class GamepadData
{
    public nint Handle;
    public readonly HashSet<SDL.GamepadButton> Buttons = new();
    public readonly Dictionary<SDL.GamepadAxis, float> RawAxes = new();
    public Vector2 LeftStick;
    public Vector2 RightStick;
    public float LeftTrigger;
    public float RightTrigger;
}
