# Gamepad

Static input for game controllers. Every call takes the controller's SDL id, so one controller's input never mixes with another's. Events carry the id too.

### Notes
* **Id-based**: get ids from `CollectConnectedIds` or from `OnConnected`. There are no per-controller objects.
* **Deadzone**: `Deadzone` (default `0.15f`, max `0.99f`) applies radial filtering to sticks and rescaling to triggers. It is global.
* **Connections**: a reconnected controller gets a new id. Drop an id when `OnDisconnected` fires, and use the new one from `OnConnected`. Calls with an unknown id return zero or `false`, and `Rumble` does nothing.

### Usage
```csharp
Gamepad.Deadzone;

Gamepad.ConnectedCount;
Gamepad.IsConnected(uint id);
Gamepad.CollectConnectedIds(List<uint> target);
Gamepad.GetConnectedIds();

Gamepad.IsButtonDown(uint id, SDL.GamepadButton button);
Gamepad.GetAxis(uint id, SDL.GamepadAxis axis);
Gamepad.LeftStick(uint id);
Gamepad.RightStick(uint id);
Gamepad.LeftTrigger(uint id);
Gamepad.RightTrigger(uint id);

Gamepad.Rumble(uint id, float lowFrequency = 0.25f, float highFrequency = 0.5f, uint durationMs = 100);

Gamepad.OnConnected;               // Action<uint id>
Gamepad.OnDisconnected;            // Action<uint id>
Gamepad.OnDown;                    // Action<uint id, SDL.GamepadButton>
Gamepad.OnUp;                      // Action<uint id, SDL.GamepadButton>
Gamepad.OnAxisMove;                // Action<uint id, SDL.GamepadAxis, float>
Gamepad.OnLeftStickMove;           // Action<uint id, Vector2>
Gamepad.OnRightStickMove;          // Action<uint id, Vector2>
Gamepad.OnLeftTriggerMove;         // Action<uint id, float>
Gamepad.OnRightTriggerMove;        // Action<uint id, float>
```

### Example
```csharp
Gamepad.OnDown += (id, button) =>
{
    if (button == SDL.GamepadButton.East) Gamepad.Rumble(id, 0.8f, 0.8f, 250);
};

Gamepad.OnLeftStickMove += (id, stick) => _players[id].Velocity = stick * 200f;

var ids = new List<uint>();
Gamepad.CollectConnectedIds(ids);
foreach (var id in ids)
{
    if (Gamepad.IsButtonDown(id, SDL.GamepadButton.South)) _players[id].Jump();
}
```
