# TouchScreen

Static touch input: active fingers and their positions in game coordinates, with down, move and up events.

### Notes
* **Driven by the `Game` loop**: positions are converted to the logical size you gave `Game`, the same space as `Mouse.Position`.
* **Fingers** are identified by a `ulong` id that lasts from touch down to touch up.
* **Mouse events**: the platform also sends mouse events for touches, so `Mouse` may still fire for a single finger.
* **Focus loss** releases every active finger and fires `OnUp` for each.

### Usage
```csharp
TouchScreen.ActiveCount;
TouchScreen.IsDown(ulong fingerId);
TouchScreen.TryGetPosition(ulong fingerId, out Vector2 position);
TouchScreen.CollectPositions(List<Vector2> buffer);

TouchScreen.OnDown;    // Action<ulong fingerId, Vector2 position>
TouchScreen.OnMove;    // Action<ulong fingerId, Vector2 position>
TouchScreen.OnUp;      // Action<ulong fingerId, Vector2 position>
```

### Example
```csharp
TouchScreen.OnDown += (id, pos) => _spawnRipple(pos);

TouchScreen.OnMove += (id, pos) =>
{
    if (TouchScreen.IsDown(id)) _drawStroke(pos);
};

TouchScreen.OnUp += (id, pos) => _endStroke(id);
```
