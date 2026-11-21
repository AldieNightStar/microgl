# Keyboard

Tells you which keys are held, which were pressed or released this frame, and turns four keys (arrows, WASD or your own) into a movement direction.

New to this? See [Tutorial 1: First window and drawing](../Tutorials/01-FirstWindow.md).

### Notes
* **Static keyboard input handler** driven automatically by the `Game` event loop. You never track keys yourself.
* **Polling**: `IsKeyDown` is true while a key is held. `IsKeyPressed` and `IsKeyReleased` are true only in the frame the key went down or up, and ignore key repeat.
* **Events**: `OnDown` and `OnUp` pass the `SDL.Keycode`. `OnDown` also fires for key repeat while a key is held.
* **Modifiers**: `IsCtrl`, `IsShift` and `IsAlt` are true while the left or right modifier is held.
* **Directions**: `GetVectorFromKeys` returns a direction with length 1 (or zero when nothing is held), so diagonal movement is not faster. `Y` grows down, like the screen. `DirectionKeys.Arrows` and `DirectionKeys.Wasd` are ready to pass.

### Usage
```csharp
// Polling
Keyboard.IsKeyDown(SDL.Keycode key);
Keyboard.IsKeyPressed(SDL.Keycode key);
Keyboard.IsKeyReleased(SDL.Keycode key);

// Direction from four keys: up, left, down, right
Keyboard.GetVectorFromKeys(DirectionKeys.Arrows);
Keyboard.GetVectorFromKeys(DirectionKeys.Wasd);
Keyboard.GetVectorFromKeys(SDL.Keycode.I, SDL.Keycode.J, SDL.Keycode.K, SDL.Keycode.L);

// Modifier key states
Keyboard.IsCtrl;
Keyboard.IsShift;
Keyboard.IsAlt;

// Key press and release events
Keyboard.OnDown;
Keyboard.OnUp;
```

### Example: movement and jumping
```csharp
private const float SPEED = 240f;

public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
{
    var direction = Keyboard.GetVectorFromKeys(DirectionKeys.Wasd);
    _player.Position += direction * SPEED * dt;

    if (Keyboard.IsKeyPressed(SDL.Keycode.Space)) _player.Jump();
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
