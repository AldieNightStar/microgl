# Mouse

### Notes
* **Static mouse input handler** driven automatically by the `Game` event loop via `ProcessEvent`.
* **Converts window coordinates to logical render space** using `SDL.ConvertEventToRenderCoordinates`, keeping `Position` accurate across window resizing and letterboxing.
* **Defines button constants** (`LEFT = 1`, `MIDDLE = 2`, `RIGHT = 3`) and exposes cursor movement and button press/release events (`OnMove`, `OnDown`, `OnUp`).

### Usage
```csharp
// Standard mouse button IDs
Mouse.LEFT; Mouse.MIDDLE; Mouse.RIGHT;

// Current cursor position in logical render coordinates
Mouse.Position;

// Cursor motion and button press/release events
Mouse.OnMove;
Mouse.OnDown;
Mouse.OnUp;
```

### Example
```csharp
Mouse.OnMove += pos => cursor.Position = pos;

Mouse.OnDown += button =>
{
    if (button == Mouse.LEFT)
    {
        Shoot(Mouse.Position);
    }
};

Mouse.OnUp += button =>
{
    if (button == Mouse.RIGHT)
    {
        EndAim();
    }
};
```
