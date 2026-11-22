# IGame

The interface your game class implements. `Game` calls its four methods for you: `Init` once, `Update` and `Draw` every frame, `Dispose` at the end.

### Notes
* **Core scene/game contract** passed into `Game`, extending `IDisposable`.
* **Defines lifecycle hooks** (`Init`, `Update`, `Draw`, and `Dispose`) driven automatically by the `Game` loop.
* **Both `Update` and `Draw` are called once per frame**: `Update` receives all `SDL.Event` items polled during that frame as a zero-allocation `ReadOnlySpan<SDL.Event>` alongside delta time `dt`.

### Usage
* `void Init(Drawer d)`
  Called once after the window and renderer are created to load assets and initialize objects.
* `void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)`
  Called once per frame after event polling with all `SDL.Event` instances collected during the frame (empty if idle) and frame delta time `dt` in seconds.
* `void Draw(Drawer d, float dt)`
  Called once per frame after `Drawer.Reset()` to render the scene with frame delta time `dt`.
* `void Dispose()`
  Inherited from `IDisposable`; called when the game closes to release textures, fonts, and objects.

### Example
```csharp
public class MyGame : IGame
{
    public void Init(Drawer d)
    {
        // Load assets
    }

    public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
    {
        foreach (var ev in events)
        {
            // Inspect raw SDL events if needed
        }

        // Update game logic once per frame
    }

    public void Draw(Drawer d, float dt)
    {
        d.DrawRect(new Rectangle(10, 10, 50, 50));
    }

    public void Dispose()
    {
        // Free assets
    }
}
```
