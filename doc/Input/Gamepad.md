# Gamepad

### Notes
* **Static gamepad input handler** backed by SDL3's unified Gamepad API (supporting Xbox, PlayStation, Switch, and standard controllers).
* **Applies radial deadzone filtering** (configurable via `Deadzone`, default `0.15f`) to analog sticks and linear deadzone rescaling to triggers.
* **Supports both polling and events** (`OnConnected`, `OnDisconnected`, `OnDown`, `OnUp`, `OnAxisMove`, `OnLeftStickMove`, `OnRightStickMove`, `OnLeftTriggerMove`, `OnRightTriggerMove`) plus controller vibration via `Rumble`.

### Usage
```csharp
// Connection state and radial deadzone threshold (0.0f to 1.0f, default 0.15f)
Gamepad.IsConnected;
Gamepad.ConnectedCount;
Gamepad.Deadzone;

// Polling buttons, individual axes, analog sticks (-1..1), and triggers (0..1)
Gamepad.IsButtonDown(SDL.GamepadButton button);
Gamepad.GetAxis(SDL.GamepadAxis axis);
Gamepad.LeftStick; Gamepad.RightStick;
Gamepad.LeftTrigger; Gamepad.RightTrigger;

// Vibrate all connected controllers with low/high motor intensities (0.0f to 1.0f) for durationMs
Gamepad.Rumble(float lowFrequency, float highFrequency, uint durationMs);

// Connection, button press/release, and filtered axis/stick/trigger events
Gamepad.OnConnected; Gamepad.OnDisconnected;
Gamepad.OnDown; Gamepad.OnUp;
Gamepad.OnAxisMove;
Gamepad.OnLeftStickMove; Gamepad.OnRightStickMove;
Gamepad.OnLeftTriggerMove; Gamepad.OnRightTriggerMove;
```

### Example
```csharp
// Event-driven input and rumble
Gamepad.OnDown += btn =>
{
    if (btn == SDL.GamepadButton.East)
    {
        Gamepad.Rumble(0.8f, 0.8f, 250);
    }
};

Gamepad.OnLeftStickMove += stick => player.Velocity = stick * 200f;

// Frame polling
if (Gamepad.IsConnected && Gamepad.IsButtonDown(SDL.GamepadButton.South))
{
    player.Jump();
}
```
