# Keyboard

### Notes
* **Static keyboard input handler** driven automatically by the `Game` event loop via `ProcessEvent`.
* **Exposes key press and release events** (`OnDown`, `OnUp`) passing `SDL.Keycode`.
* **Tracks modifier key states** (`IsCtrl`, `IsShift`, `IsAlt`) across both left and right modifier keys.

### Usage
```csharp
// Key press and release events
Keyboard.OnDown;
Keyboard.OnUp;

// Current modifier key states (true while either Left or Right modifier is held)
Keyboard.IsCtrl;
Keyboard.IsShift;
Keyboard.IsAlt;
```

### Example
```csharp
Keyboard.OnDown += key =>
{
    if (key == SDL.Keycode.S && Keyboard.IsCtrl)
    {
        SaveGame();
    }
};

Keyboard.OnUp += key =>
{
    if (key == SDL.Keycode.Space)
    {
        StopCharging();
    }
};
```
