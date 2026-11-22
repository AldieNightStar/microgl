using System.Numerics;
using SDL3;

namespace microgl;

public static class Gamepad
{
    private const float DEFAULT_DEADZONE = 0.15f;
    private const float MAX_DEADZONE = 0.99f;
    private const float POSITIVE_AXIS_MAX = 32767f;
    private const float NEGATIVE_AXIS_MAX = 32768f;
    private const float TRIGGER_MAX = 32767f;

    private static readonly uint[] _gamepadEvents =
    [
        (uint)SDL.EventType.GamepadAdded,
        (uint)SDL.EventType.GamepadRemoved,
        (uint)SDL.EventType.GamepadButtonDown,
        (uint)SDL.EventType.GamepadButtonUp,
        (uint)SDL.EventType.GamepadAxisMotion
    ];

    private static readonly Dictionary<uint, GamepadData> _pads = new();

    public static float Deadzone
    {
        get => field;
        set => field = Math.Clamp(value, 0f, MAX_DEADZONE);
    } = DEFAULT_DEADZONE;

    public static event Action<uint>? OnConnected;
    public static event Action<uint>? OnDisconnected;
    public static event Action<uint, SDL.GamepadButton>? OnDown;
    public static event Action<uint, SDL.GamepadButton>? OnUp;
    public static event Action<uint, SDL.GamepadAxis, float>? OnAxisMove;
    public static event Action<uint, Vector2>? OnLeftStickMove;
    public static event Action<uint, Vector2>? OnRightStickMove;
    public static event Action<uint, float>? OnLeftTriggerMove;
    public static event Action<uint, float>? OnRightTriggerMove;

    public static void CollectConnectedIds(List<uint> target)
    {
        target.Clear();
        foreach (var id in _pads.Keys) target.Add(id);
    }

    public static int ConnectedCount => _pads.Count;
    public static bool IsConnected(uint id) => _pads.ContainsKey(id);
    public static IEnumerable<uint> GetConnectedIds() => _pads.Keys;

    public static bool IsButtonDown(uint id, SDL.GamepadButton button) =>
        _pads.TryGetValue(id, out var pad) && pad.Buttons.Contains(button);

    public static float GetAxis(uint id, SDL.GamepadAxis axis) =>
        _pads.TryGetValue(id, out var pad) ? _axisValue(pad, axis) : 0f;

    public static Vector2 LeftStick(uint id) => _pads.TryGetValue(id, out var pad) ? pad.LeftStick : Vector2.Zero;
    public static Vector2 RightStick(uint id) => _pads.TryGetValue(id, out var pad) ? pad.RightStick : Vector2.Zero;
    public static float LeftTrigger(uint id) => _pads.TryGetValue(id, out var pad) ? pad.LeftTrigger : 0f;
    public static float RightTrigger(uint id) => _pads.TryGetValue(id, out var pad) ? pad.RightTrigger : 0f;

    public static void Rumble(uint id, float lowFrequency=0.25f, float highFrequency=0.5f, uint durationMs=100)
    {
        if (!_pads.TryGetValue(id, out var pad)) return;

        ushort low = (ushort)(Math.Clamp(lowFrequency, 0f, 1f) * ushort.MaxValue);
        ushort high = (ushort)(Math.Clamp(highFrequency, 0f, 1f) * ushort.MaxValue);
        SDL.RumbleGamepad(pad.Handle, low, high, durationMs);
    }

    internal static bool IsGamepadEvent(ref SDL.Event ev) =>
        Array.IndexOf(_gamepadEvents, ev.Type) >= 0;

    internal static void ProcessEvent(ref SDL.Event ev)
    {
        switch ((SDL.EventType)ev.Type)
        {
            case SDL.EventType.GamepadAdded: _onAdded(ev.GDevice.Which); break;
            case SDL.EventType.GamepadRemoved: _onRemoved(ev.GDevice.Which); break;
            case SDL.EventType.GamepadButtonDown: _onButton(ev.GButton.Which, (SDL.GamepadButton)ev.GButton.Button, true); break;
            case SDL.EventType.GamepadButtonUp: _onButton(ev.GButton.Which, (SDL.GamepadButton)ev.GButton.Button, false); break;
            case SDL.EventType.GamepadAxisMotion: _onAxis(ev.GAxis.Which, (SDL.GamepadAxis)ev.GAxis.Axis, ev.GAxis.Value); break;
        }
    }

    internal static void DisposeAll()
    {
        foreach (var pad in _pads.Values) _close(pad);
        _pads.Clear();
    }

    private static void _onAdded(uint id)
    {
        if (_pads.ContainsKey(id)) return;

        nint handle = SDL.OpenGamepad(id);
        if (handle == IntPtr.Zero) return;

        _pads[id] = new GamepadData { Handle = handle };
        OnConnected?.Invoke(id);
    }

    private static void _onRemoved(uint id)
    {
        if (_pads.Remove(id, out var pad)) _close(pad);
        OnDisconnected?.Invoke(id);
    }

    private static void _close(GamepadData pad)
    {
        if (pad.Handle != IntPtr.Zero) SDL.CloseGamepad(pad.Handle);
        pad.Handle = IntPtr.Zero;
    }

    private static void _onButton(uint id, SDL.GamepadButton button, bool down)
    {
        if (!_pads.TryGetValue(id, out var pad)) return;

        if (down)
        {
            pad.Buttons.Add(button);
            OnDown?.Invoke(id, button);
        }
        else
        {
            pad.Buttons.Remove(button);
            OnUp?.Invoke(id, button);
        }
    }

    private static void _onAxis(uint id, SDL.GamepadAxis axis, short rawValue)
    {
        if (!_pads.TryGetValue(id, out var pad)) return;

        float rawNorm = _normalizeRaw(axis, rawValue);
        pad.RawAxes[axis] = rawNorm;

        if (axis is SDL.GamepadAxis.LeftX or SDL.GamepadAxis.LeftY)
            _updateStick(id, pad, axis, SDL.GamepadAxis.LeftX, SDL.GamepadAxis.LeftY, ref pad.LeftStick, OnLeftStickMove);
        else if (axis is SDL.GamepadAxis.RightX or SDL.GamepadAxis.RightY)
            _updateStick(id, pad, axis, SDL.GamepadAxis.RightX, SDL.GamepadAxis.RightY, ref pad.RightStick, OnRightStickMove);
        else if (axis == SDL.GamepadAxis.LeftTrigger)
            _updateTrigger(id, axis, rawNorm, ref pad.LeftTrigger, OnLeftTriggerMove);
        else if (axis == SDL.GamepadAxis.RightTrigger)
            _updateTrigger(id, axis, rawNorm, ref pad.RightTrigger, OnRightTriggerMove);
    }

    private static void _updateStick(
        uint id, GamepadData pad, SDL.GamepadAxis axis, SDL.GamepadAxis xAxis, SDL.GamepadAxis yAxis,
        ref Vector2 stick, Action<uint, Vector2>? onMove)
    {
        var raw = new Vector2(pad.RawAxes.GetValueOrDefault(xAxis), pad.RawAxes.GetValueOrDefault(yAxis));
        var filtered = _applyRadialDeadzone(raw);
        if (filtered == stick) return;

        stick = filtered;
        OnAxisMove?.Invoke(id, axis, axis == xAxis ? filtered.X : filtered.Y);
        onMove?.Invoke(id, filtered);
    }

    private static void _updateTrigger(uint id, SDL.GamepadAxis axis, float rawNorm, ref float trigger, Action<uint, float>? onMove)
    {
        float filtered = rawNorm < Deadzone ? 0f : (rawNorm - Deadzone) / (1f - Deadzone);
        if (filtered == trigger) return;

        trigger = filtered;
        OnAxisMove?.Invoke(id, axis, filtered);
        onMove?.Invoke(id, filtered);
    }

    private static float _axisValue(GamepadData pad, SDL.GamepadAxis axis) => axis switch
    {
        SDL.GamepadAxis.LeftX => pad.LeftStick.X,
        SDL.GamepadAxis.LeftY => pad.LeftStick.Y,
        SDL.GamepadAxis.RightX => pad.RightStick.X,
        SDL.GamepadAxis.RightY => pad.RightStick.Y,
        SDL.GamepadAxis.LeftTrigger => pad.LeftTrigger,
        SDL.GamepadAxis.RightTrigger => pad.RightTrigger,
        _ => pad.RawAxes.TryGetValue(axis, out float val) ? val : 0f
    };

    private static Vector2 _applyRadialDeadzone(Vector2 stick)
    {
        float len = stick.Length();
        if (len < Deadzone) return Vector2.Zero;

        float rescaled = Math.Clamp((len - Deadzone) / (1f - Deadzone), 0f, 1f);
        return (stick / len) * rescaled;
    }

    private static float _normalizeRaw(SDL.GamepadAxis axis, short rawValue)
    {
        if (axis is SDL.GamepadAxis.LeftTrigger or SDL.GamepadAxis.RightTrigger)
            return Math.Clamp(rawValue / TRIGGER_MAX, 0f, 1f);

        float norm = rawValue < 0 ? rawValue / NEGATIVE_AXIS_MAX : rawValue / POSITIVE_AXIS_MAX;
        return Math.Clamp(norm, -1f, 1f);
    }
}
