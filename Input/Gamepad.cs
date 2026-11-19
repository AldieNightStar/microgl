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
    private static readonly Dictionary<SDL.GamepadAxis, float> _rawAxes = new();

    private static Vector2 _leftStick;
    private static Vector2 _rightStick;
    private static float _leftTrigger;
    private static float _rightTrigger;

    public static float Deadzone { get; set; } = 0.15f;
    public static bool IsConnected => _openGamepads.Count > 0;
    public static int ConnectedCount => _openGamepads.Count;

    // Connection & Button Events
    public static event Action<uint>? OnConnected;
    public static event Action<uint>? OnDisconnected;
    public static event Action<SDL.GamepadButton>? OnDown;
    public static event Action<SDL.GamepadButton>? OnUp;

    // Axis, Stick & Trigger Events
    public static event Action<SDL.GamepadAxis, float>? OnAxisMove;
    public static event Action<Vector2>? OnLeftStickMove;
    public static event Action<Vector2>? OnRightStickMove;
    public static event Action<float>? OnLeftTriggerMove;
    public static event Action<float>? OnRightTriggerMove;

    // Stick & Trigger Properties
    public static Vector2 LeftStick => _leftStick;
    public static Vector2 RightStick => _rightStick;
    public static float LeftTrigger => _leftTrigger;
    public static float RightTrigger => _rightTrigger;

    public static bool IsButtonDown(SDL.GamepadButton button) => _pressedButtons.Contains(button);

    public static float GetAxis(SDL.GamepadAxis axis) => axis switch
    {
        SDL.GamepadAxis.LeftX => _leftStick.X,
        SDL.GamepadAxis.LeftY => _leftStick.Y,
        SDL.GamepadAxis.RightX => _rightStick.X,
        SDL.GamepadAxis.RightY => _rightStick.Y,
        SDL.GamepadAxis.LeftTrigger => _leftTrigger,
        SDL.GamepadAxis.RightTrigger => _rightTrigger,
        _ => _rawAxes.TryGetValue(axis, out float val) ? val : 0f
    };

    /// <summary>
    /// Vibrates all connected gamepads (0.0 to 1.0 intensity).
    /// </summary>
    public static void Rumble(float lowFrequency, float highFrequency, uint durationMs)
    {
        ushort low = (ushort)(Math.Clamp(lowFrequency, 0f, 1f) * ushort.MaxValue);
        ushort high = (ushort)(Math.Clamp(highFrequency, 0f, 1f) * ushort.MaxValue);

        foreach (var handle in _openGamepads.Values)
            SDL.RumbleGamepad(handle, low, high, durationMs);
    }

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
                    SDL.CloseGamepad(handle);

                if (_openGamepads.Count == 0)
                {
                    _pressedButtons.Clear();
                    _rawAxes.Clear();
                    _leftStick = Vector2.Zero;
                    _rightStick = Vector2.Zero;
                    _leftTrigger = 0f;
                    _rightTrigger = 0f;
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
                float rawNorm = NormalizeRaw(axis, ev.GAxis.Value);
                _rawAxes[axis] = rawNorm;

                switch (axis)
                {
                    case SDL.GamepadAxis.LeftX or SDL.GamepadAxis.LeftY:
                    {
                        var rawVec = new Vector2(
                            _rawAxes.GetValueOrDefault(SDL.GamepadAxis.LeftX),
                            _rawAxes.GetValueOrDefault(SDL.GamepadAxis.LeftY));
                        var filtered = ApplyRadialDeadzone(rawVec);

                        if (filtered != _leftStick)
                        {
                            _leftStick = filtered;
                            OnAxisMove?.Invoke(axis, axis == SDL.GamepadAxis.LeftX ? filtered.X : filtered.Y);
                            OnLeftStickMove?.Invoke(filtered);
                        }
                        break;
                    }

                    case SDL.GamepadAxis.RightX or SDL.GamepadAxis.RightY:
                    {
                        var rawVec = new Vector2(
                            _rawAxes.GetValueOrDefault(SDL.GamepadAxis.RightX),
                            _rawAxes.GetValueOrDefault(SDL.GamepadAxis.RightY));
                        var filtered = ApplyRadialDeadzone(rawVec);

                        if (filtered != _rightStick)
                        {
                            _rightStick = filtered;
                            OnAxisMove?.Invoke(axis, axis == SDL.GamepadAxis.RightX ? filtered.X : filtered.Y);
                            OnRightStickMove?.Invoke(filtered);
                        }
                        break;
                    }

                    case SDL.GamepadAxis.LeftTrigger:
                    {
                        float filtered = rawNorm < Deadzone ? 0f : (rawNorm - Deadzone) / (1f - Deadzone);
                        if (filtered != _leftTrigger)
                        {
                            _leftTrigger = filtered;
                            OnAxisMove?.Invoke(axis, filtered);
                            OnLeftTriggerMove?.Invoke(filtered);
                        }
                        break;
                    }

                    case SDL.GamepadAxis.RightTrigger:
                    {
                        float filtered = rawNorm < Deadzone ? 0f : (rawNorm - Deadzone) / (1f - Deadzone);
                        if (filtered != _rightTrigger)
                        {
                            _rightTrigger = filtered;
                            OnAxisMove?.Invoke(axis, filtered);
                            OnRightTriggerMove?.Invoke(filtered);
                        }
                        break;
                    }
                }
                break;
            }
        }
    }

    private static float NormalizeRaw(SDL.GamepadAxis axis, short rawValue)
    {
        if (axis is SDL.GamepadAxis.LeftTrigger or SDL.GamepadAxis.RightTrigger)
            return Math.Clamp(rawValue / 32767f, 0f, 1f);

        float norm = rawValue < 0 ? rawValue / 32768f : rawValue / 32767f;
        return Math.Clamp(norm, -1f, 1f);
    }

    private static Vector2 ApplyRadialDeadzone(Vector2 stick)
    {
        float len = stick.Length();
        if (len < Deadzone) return Vector2.Zero;

        float rescaled = Math.Clamp((len - Deadzone) / (1f - Deadzone), 0f, 1f);
        return (stick / len) * rescaled;
    }

    internal static void DisposeAll()
    {
        foreach (var handle in _openGamepads.Values)
            SDL.CloseGamepad(handle);

        _openGamepads.Clear();
        _pressedButtons.Clear();
        _rawAxes.Clear();
        _leftStick = Vector2.Zero;
        _rightStick = Vector2.Zero;
        _leftTrigger = 0f;
        _rightTrigger = 0f;
    }
}
