# IGame

### Notes
* **Core scene/game contract** passed into `Game`, extending `IDisposable`.
* **Defines lifecycle hooks** (`Init`, `Update`, `Draw`, and `Dispose`) driven automatically by the `Game` loop.
* **`Update` is called per SDL event** during a frame (or once with `default` event if no events occurred), while **`Draw` is called once per frame**.

### Usage
* `void Init(Drawer d)`
  Called once after the window and renderer are created to load assets and initialize objects.
* `void Update(Drawer d, ref SDL.Event ev, float dt)`
  Called during event polling with the current `SDL.Event` (or `default` if idle) and frame delta time `dt` in seconds.
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

    public void Update(Drawer d, ref SDL.Event ev, float dt)
    {
        // Handle logic
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
