# Game

Opens the window and runs your game. Create one in `Main`, pass it your `IGame`, and call `Start()`.

### Notes
* **Entry point and lifecycle manager** for a `microgl` application, implementing `IDisposable`.
* **Initializes SDL3 subsystems** (`Video`, `Gamepad` and `Audio`), creates a resizable window, configures `Nearest` pixel-art scaling with `Letterbox` logical presentation, and sets `Drawer.ScreenSize` to the logical window dimensions.
* **Runs the main loop** by ticking `Time`, routing input events (`Mouse`, `Keyboard`, `Gamepad`, `TouchScreen`), and releasing held input when the window loses focus. Each frame it calls `IGame.Update` (with the frame's events as a `ReadOnlySpan<SDL.Event>`), then `Drawer.Reset()`, then `IGame.Draw`.
* **VSync is on by default** (`VSync = true`), capping the loop at the display refresh rate. It is applied once at startup, so set it before `Start()`. `false` gives an uncapped loop that uses more CPU.
* **Automatically cleans up** game resources, text engine, audio, renderer, connected gamepads, the GPU device and the native window when the loop exits.

### Usage
```csharp
// Create a game instance with window title, logical resolution (w x h), and scene implementation
new Game(string title, int w, int h, IGame game);

// Cap the loop at the display refresh rate (default true); set before Start()
game.VSync = true;

// Initialize SDL, open the window, run the blocking main loop, and clean up on exit
game.Start();

// Release everything; Start() already does this, so a second call does nothing
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
