using System.Numerics;
using SDL3;

namespace microgl;

public static class Gamepad
{
    private const float _defaultDeadzone = 0.15f;
    private const float _maxDeadzone = 0.99f;
    private const float _positiveAxisMax = 32767f;
    private const float _negativeAxisMax = 32768f;
    private const float _triggerMax = 32767f;

    private static readonly uint[] _gamepadEvents =
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

    public static float Deadzone
    {
        get => field;
        set => field = Math.Clamp(value, 0f, _maxDeadzone);
    } = _defaultDeadzone;

    public static bool IsConnected => _openGamepads.Count > 0;
    public static int ConnectedCount => _openGamepads.Count;

    public static event Action<uint>? OnConnected;
    public static event Action<uint>? OnDisconnected;
    public static event Action<SDL.GamepadButton>? OnDown;
    public static event Action<SDL.GamepadButton>? OnUp;

    public static event Action<SDL.GamepadAxis, float>? OnAxisMove;
    public static event Action<Vector2>? OnLeftStickMove;
    public static event Action<Vector2>? OnRightStickMove;
    public static event Action<float>? OnLeftTriggerMove;
    public static event Action<float>? OnRightTriggerMove;

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

    public static void Rumble(float lowFrequency, float highFrequency, uint durationMs)
    {
        ushort low = (ushort)(Math.Clamp(lowFrequency, 0f, 1f) * ushort.MaxValue);
        ushort high = (ushort)(Math.Clamp(highFrequency, 0f, 1f) * ushort.MaxValue);

        foreach (var handle in _openGamepads.Values)
            SDL.RumbleGamepad(handle, low, high, durationMs);
    }

    internal static bool IsGamepadEvent(ref SDL.Event ev) =>
        Array.IndexOf(_gamepadEvents, ev.Type) >= 0;

    internal static void ProcessEvent(ref SDL.Event ev)
    {
        switch ((SDL.EventType)ev.Type)
        {
            case SDL.EventType.GamepadAdded: _onAdded(ev.GDevice.Which); break;
            case SDL.EventType.GamepadRemoved: _onRemoved(ev.GDevice.Which); break;
            case SDL.EventType.GamepadButtonDown: _onButtonDown((SDL.GamepadButton)ev.GButton.Button); break;
            case SDL.EventType.GamepadButtonUp: _onButtonUp((SDL.GamepadButton)ev.GButton.Button); break;
            case SDL.EventType.GamepadAxisMotion: _onAxisMotion((SDL.GamepadAxis)ev.GAxis.Axis, ev.GAxis.Value); break;
        }
    }

    internal static void DisposeAll()
    {
        foreach (var handle in _openGamepads.Values)
            SDL.CloseGamepad(handle);

        _openGamepads.Clear();
        _resetState();
    }

    private static void _onAdded(uint instanceId)
    {
        nint handle = SDL.OpenGamepad(instanceId);
        if (handle == IntPtr.Zero) return;

        _openGamepads[instanceId] = handle;
        OnConnected?.Invoke(instanceId);
    }

    private static void _onRemoved(uint instanceId)
    {
        if (_openGamepads.Remove(instanceId, out nint handle))
            SDL.CloseGamepad(handle);

        if (_openGamepads.Count == 0) _resetState();
        OnDisconnected?.Invoke(instanceId);
    }

    private static void _onButtonDown(SDL.GamepadButton button)
    {
        _pressedButtons.Add(button);
        OnDown?.Invoke(button);
    }

    private static void _onButtonUp(SDL.GamepadButton button)
    {
        _pressedButtons.Remove(button);
        OnUp?.Invoke(button);
    }

    private static void _onAxisMotion(SDL.GamepadAxis axis, short rawValue)
    {
        float rawNorm = _normalizeRaw(axis, rawValue);
        _rawAxes[axis] = rawNorm;

        if (axis is SDL.GamepadAxis.LeftX or SDL.GamepadAxis.LeftY)
            _updateStick(axis, SDL.GamepadAxis.LeftX, SDL.GamepadAxis.LeftY, ref _leftStick, OnLeftStickMove);
        else if (axis is SDL.GamepadAxis.RightX or SDL.GamepadAxis.RightY)
            _updateStick(axis, SDL.GamepadAxis.RightX, SDL.GamepadAxis.RightY, ref _rightStick, OnRightStickMove);
        else if (axis == SDL.GamepadAxis.LeftTrigger)
            _updateTrigger(axis, rawNorm, ref _leftTrigger, OnLeftTriggerMove);
        else if (axis == SDL.GamepadAxis.RightTrigger)
            _updateTrigger(axis, rawNorm, ref _rightTrigger, OnRightTriggerMove);
    }

    private static void _updateStick(
        SDL.GamepadAxis axis, SDL.GamepadAxis xAxis, SDL.GamepadAxis yAxis,
        ref Vector2 stick, Action<Vector2>? onMove)
    {
        var raw = new Vector2(_rawAxes.GetValueOrDefault(xAxis), _rawAxes.GetValueOrDefault(yAxis));
        var filtered = _applyRadialDeadzone(raw);
        if (filtered == stick) return;

        stick = filtered;
        OnAxisMove?.Invoke(axis, axis == xAxis ? filtered.X : filtered.Y);
        onMove?.Invoke(filtered);
    }

    private static void _updateTrigger(SDL.GamepadAxis axis, float rawNorm, ref float trigger, Action<float>? onMove)
    {
        float filtered = rawNorm < Deadzone ? 0f : (rawNorm - Deadzone) / (1f - Deadzone);
        if (filtered == trigger) return;

        trigger = filtered;
        OnAxisMove?.Invoke(axis, filtered);
        onMove?.Invoke(filtered);
    }

    private static void _resetState()
    {
        _pressedButtons.Clear();
        _rawAxes.Clear();
        _leftStick = Vector2.Zero;
        _rightStick = Vector2.Zero;
        _leftTrigger = 0f;
        _rightTrigger = 0f;
    }

    private static float _normalizeRaw(SDL.GamepadAxis axis, short rawValue)
    {
        if (axis is SDL.GamepadAxis.LeftTrigger or SDL.GamepadAxis.RightTrigger)
            return Math.Clamp(rawValue / _triggerMax, 0f, 1f);

        float norm = rawValue < 0 ? rawValue / _negativeAxisMax : rawValue / _positiveAxisMax;
        return Math.Clamp(norm, -1f, 1f);
    }

    private static Vector2 _applyRadialDeadzone(Vector2 stick)
    {
        float len = stick.Length();
        if (len < Deadzone) return Vector2.Zero;

        float rescaled = Math.Clamp((len - Deadzone) / (1f - Deadzone), 0f, 1f);
        return (stick / len) * rescaled;
    }
}
