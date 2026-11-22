# Keyboard

Static keyboard input: which keys are held, key press and release events, and a movement direction from four keys.

### Notes
* **Driven by the `Game` loop**: you never track keys yourself.
* **`IsKeyDown`** is true while a key is held. **`IsKeyUp`** is true while it is not.
* **`OnDown`** fires once per press and **`OnUp`** on release. Key repeat is ignored.
* **Modifiers**: `IsCtrl`, `IsShift` and `IsAlt` are true while either side's key is held.
* **Directions**: `GetVectorFromKeys` returns a vector of length 1, or zero when nothing is held, so diagonal movement is not faster. `Y` grows down, like the screen.
* **Focus loss** releases every held key and fires `OnUp` for each.

### Usage
```csharp
Keyboard.IsKeyDown(SDL.Keycode key);
Keyboard.IsKeyUp(SDL.Keycode key);

Keyboard.IsCtrl;
Keyboard.IsShift;
Keyboard.IsAlt;

Keyboard.GetVectorFromKeys(DirectionKeys keys);
Keyboard.OnDown;
Keyboard.OnUp;

DirectionKeys.Arrows;
DirectionKeys.WASD;
new DirectionKeys(SDL.Keycode up, SDL.Keycode left, SDL.Keycode down, SDL.Keycode right);
```

### Example: movement
```csharp
private const float Speed = 240f;

public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
{
    var direction = Keyboard.GetVectorFromKeys(DirectionKeys.WASD);
    _player.Position += direction * Speed * dt;
}
```

### Example: events
```csharp
Keyboard.OnDown += key =>
{
    if (key == SDL.Keycode.S && Keyboard.IsCtrl)
    {
        SaveGame();
    }
};
```
