# Game

Opens the window and runs your game. Create one in `Main`, pass it your `IGame`, and call `Start()`.

New to this? See [Tutorial 1: First window and drawing](Tutorials/01-FirstWindow.md).

### Notes
* **Entry point and lifecycle manager** for a `microgl` application, implementing `IDisposable`.
* **Initializes SDL3 subsystems** (`Video` and `Gamepad`), creates a resizable window, configures `Nearest` pixel-art scaling with `Letterbox` logical presentation, and sets `Drawer.ScreenSize` to the logical window dimensions.
* **Runs the main loop** by ticking `Time`, routing input events (`Mouse`, `Keyboard`, `Gamepad`), batching frame events without heap allocations, and calling `IGame.Update` (with `ReadOnlySpan<SDL.Event>`) and `IGame.Draw` once per frame.
* **VSync is on by default** (`VSync = true`), capping the loop at the display refresh rate. Set it to `false` before `Start()` for an uncapped loop, which burns CPU and multiplies per-frame allocations.
* **Automatically cleans up** game resources, renderer, connected gamepads, and the native window when the loop exits.

### Usage
```csharp
// Create a game instance with window title, logical resolution (w x h), and scene implementation
new Game(string title, int w, int h, IGame game);

// Cap the loop at the display refresh rate (default true); set before Start()
game.VSync = true;

// Initialize SDL, open the window, run the blocking main loop, and clean up on exit
game.Start();

// Manually release IGame, Drawer, active gamepads, and window handle
game.Dispose();
```

### Example
```csharp
public static void Main()
{
    var game = new Game("My Game", 720, 480, new MyGame());
    game.Start();
}
```

`Start()` returns when the window is closed, and has already disposed your game and the window by then.

The window can be resized freely. The picture is scaled to fit and keeps its shape (letterboxing), so you always draw in the size you passed, here 720x480. `Mouse.Position` is in that size too.
