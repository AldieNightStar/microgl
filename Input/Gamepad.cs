using System.Numerics;
using SDL3;

namespace microgl;

public static class Gamepad
{
    private static readonly uint[] GAMEPAD_EVENTS =
    [
        (uint)SDL.EventType.GamepadAdded,
        (uint)SDL.EventType.GamepadRemoved,
        (uint)SDL.EventType.GamepadButtonDown,
        (uint)SDL.EventType.GamepadButtonUp,
        (uint)SDL.EventType.GamepadAxisMotion
    ];

    private static readonly Dictionary<uint, nint> _openGamepads = new();
    private static readonly HashSet<SDL.GamepadButton> _pressedButtons = new();
    private static readonly Dictionary<SDL.GamepadAxis, float> _axes = new();

    public static float Deadzone { get; set; } = 0.15f;
    public static bool IsConnected => _openGamepads.Count > 0;
    public static int ConnectedCount => _openGamepads.Count;

    // Events
    public static event Action<uint>? OnConnected;
    public static event Action<uint>? OnDisconnected;
    public static event Action<SDL.GamepadButton>? OnDown;
    public static event Action<SDL.GamepadButton>? OnUp;
    public static event Action<SDL.GamepadAxis, float>? OnAxisMove;

    // High-level Stick & Trigger Events
    public static event Action<Vector2>? OnLeftStickMove;
    public static event Action<Vector2>? OnRightStickMove;
    public static event Action<float>? OnLeftTriggerMove;
    public static event Action<float>? OnRightTriggerMove;

    // Stick & Trigger helpers (normalized -1..1 for sticks, 0..1 for triggers)
    public static Vector2 LeftStick => new(GetAxis(SDL.GamepadAxis.LeftX), GetAxis(SDL.GamepadAxis.LeftY));
    public static Vector2 RightStick => new(GetAxis(SDL.GamepadAxis.RightX), GetAxis(SDL.GamepadAxis.RightY));
    public static float LeftTrigger => GetAxis(SDL.GamepadAxis.LeftTrigger);
    public static float RightTrigger => GetAxis(SDL.GamepadAxis.RightTrigger);

    public static bool IsButtonDown(SDL.GamepadButton button) => _pressedButtons.Contains(button);

    public static float GetAxis(SDL.GamepadAxis axis) =>
        _axes.TryGetValue(axis, out float val) ? val : 0f;

    internal static bool IsGamepadEvent(ref SDL.Event ev) =>
        Array.IndexOf(GAMEPAD_EVENTS, ev.Type) >= 0;

    internal static void ProcessEvent(ref SDL.Event ev)
    {
        var type = (SDL.EventType)ev.Type;

        switch (type)
        {
            case SDL.EventType.GamepadAdded:
            {
                uint instanceId = ev.GDevice.Which;
                nint handle = SDL.OpenGamepad(instanceId);
                if (handle != IntPtr.Zero)
                {
                    _openGamepads[instanceId] = handle;
                    OnConnected?.Invoke(instanceId);
                }
                break;
            }

            case SDL.EventType.GamepadRemoved:
            {
                uint instanceId = ev.GDevice.Which;
                if (_openGamepads.Remove(instanceId, out nint handle))
                {
                    SDL.CloseGamepad(handle);
                }

                if (_openGamepads.Count == 0)
                {
                    _pressedButtons.Clear();
                    _axes.Clear();
                }

                OnDisconnected?.Invoke(instanceId);
                break;
            }

            case SDL.EventType.GamepadButtonDown:
            {
                var button = (SDL.GamepadButton)ev.GButton.Button;
                _pressedButtons.Add(button);
                OnDown?.Invoke(button);
                break;
            }

            case SDL.EventType.GamepadButtonUp:
            {
                var button = (SDL.GamepadButton)ev.GButton.Button;
                _pressedButtons.Remove(button);
                OnUp?.Invoke(button);
                break;
            }

            case SDL.EventType.GamepadAxisMotion:
            {
                var axis = (SDL.GamepadAxis)ev.GAxis.Axis;
                float prev = GetAxis(axis);
                float normalized = NormalizeAxis(axis, ev.GAxis.Value);

                if (prev == normalized)
                    break;

                _axes[axis] = normalized;
                OnAxisMove?.Invoke(axis, normalized);

                switch (axis)
                {
                    case SDL.GamepadAxis.LeftX or SDL.GamepadAxis.LeftY:
                        OnLeftStickMove?.Invoke(LeftStick);
                        break;
                    case SDL.GamepadAxis.RightX or SDL.GamepadAxis.RightY:
                        OnRightStickMove?.Invoke(RightStick);
                        break;
                    case SDL.GamepadAxis.LeftTrigger:
                        OnLeftTriggerMove?.Invoke(normalized);
                        break;
                    case SDL.GamepadAxis.RightTrigger:
                        OnRightTriggerMove?.Invoke(normalized);
                        break;
                }
                break;
            }
        }
    }

    private static float NormalizeAxis(SDL.GamepadAxis axis, short rawValue)
    {
        if (axis is SDL.GamepadAxis.LeftTrigger or SDL.GamepadAxis.RightTrigger)
        {
            float v = Math.Clamp(rawValue / 32767f, 0f, 1f);
            return v < Deadzone ? 0f : v;
        }

        float norm = rawValue < 0 ? rawValue / 32768f : rawValue / 32767f;
        norm = Math.Clamp(norm, -1f, 1f);
        return MathF.Abs(norm) < Deadzone ? 0f : norm;
    }

    internal static void DisposeAll()
    {
        foreach (var handle in _openGamepads.Values)
            SDL.CloseGamepad(handle);

        _openGamepads.Clear();
        _pressedButtons.Clear();
        _axes.Clear();
    }
}
