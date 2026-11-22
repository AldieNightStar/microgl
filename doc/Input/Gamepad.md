# Gamepad

Static input for game controllers. Every call takes the controller's SDL id, so input from one controller never mixes with another. Events also carry that id.

New to this? See [Tutorial 1: First window and drawing](../Tutorials/01-FirstWindow.md).

### Notes
* **Id-based static API**: pass the id from `CollectConnectedIds` or from a connection event. No state objects. No allocations per frame.
* **Radial deadzone** on sticks and linear rescaling on triggers, set globally with `Deadzone` (default `0.15f`).
* **Events** carry the controller id as the first argument.

### Usage
```csharp
// Connected controllers
Gamepad.CollectConnectedIds(List<uint> target);   // fills your list, allocates nothing
Gamepad.ConnectedCount;
Gamepad.IsConnected(uint id);
Gamepad.Deadzone;                  // 0.0f to 0.99f

// Polling per controller
Gamepad.IsButtonDown(uint id, SDL.GamepadButton button);
Gamepad.GetAxis(uint id, SDL.GamepadAxis axis);     // -1..1 for sticks, 0..1 for triggers
Gamepad.LeftStick(uint id);
Gamepad.RightStick(uint id);
Gamepad.LeftTrigger(uint id);
Gamepad.RightTrigger(uint id);

// Vibration for one controller (low/high motor 0.0f to 1.0f for durationMs)
Gamepad.Rumble(uint id, float lowFrequency, float highFrequency, uint durationMs);

// Events
Gamepad.OnConnected;               // Action<uint id>
Gamepad.OnDisconnected;            // Action<uint id>
Gamepad.OnDown; Gamepad.OnUp;      // Action<uint id, SDL.GamepadButton>
Gamepad.OnAxisMove;                // Action<uint id, SDL.GamepadAxis, float>
Gamepad.OnLeftStickMove; Gamepad.OnRightStickMove;     // Action<uint id, Vector2>
Gamepad.OnLeftTriggerMove; Gamepad.OnRightTriggerMove; // Action<uint id, float>
```

### Example
```csharp
Gamepad.OnDown += (id, btn) =>
{
    if (btn == SDL.GamepadButton.East) Gamepad.Rumble(id, 0.8f, 0.8f, 250);
};

Gamepad.OnLeftStickMove += (id, stick) => players[id].Velocity = stick * 200f;

// Frame polling
Gamepad.CollectConnectedIds(connectedIds);
foreach (var id in connectedIds)
{
    if (Gamepad.IsButtonDown(id, SDL.GamepadButton.South)) players[id].Jump();
}
```
