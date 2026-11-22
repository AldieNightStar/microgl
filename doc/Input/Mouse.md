# Mouse

Static mouse input: the cursor position in game coordinates, and button and wheel events.

### Notes
* **Driven by the `Game` loop**: events are converted to the logical size you gave `Game`, so `Position` stays correct when the window is resized or letterboxed.
* **Buttons**: `Mouse.LEFT`, `Mouse.MIDDLE` and `Mouse.RIGHT`.
* **Polling**: `IsDown(button)` is true while a button is held.
* **Focus loss** releases every held button and fires `OnUp` for each.

### Usage
```csharp
Mouse.Position;
Mouse.IsDown(uint button);

Mouse.OnMove;    // Action<Vector2>
Mouse.OnDown;    // Action<uint button>
Mouse.OnUp;      // Action<uint button>
Mouse.OnWheel;   // Action<Vector2>
```

### Example
```csharp
Mouse.OnMove += pos => _cursor.Position = pos;

Mouse.OnDown += button =>
{
    if (button == Mouse.LEFT) Shoot(Mouse.Position);
};

Mouse.OnUp += button =>
{
    if (button == Mouse.RIGHT) EndAim();
};
```
