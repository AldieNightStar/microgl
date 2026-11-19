# Game

### Notes
* **Entry point and lifecycle manager** for a `microgl` application, implementing `IDisposable`.
* **Initializes SDL3 subsystems** (`Video` and `Gamepad`), creates a resizable window, configures `Nearest` pixel-art scaling with `Letterbox` logical presentation, and sets `Drawer.ScreenSize` to the logical window dimensions.
* **Runs the main loop** by ticking `Time`, routing input events (`Mouse`, `Keyboard`, `Gamepad`), batching frame events without heap allocations, and calling `IGame.Update` (with `ReadOnlySpan<SDL.Event>`) and `IGame.Draw` once per frame.
* **Automatically cleans up** game resources, renderer, gamepads, and the native window when the loop exits.

### Usage
```csharp
// Create a game instance with window title, logical resolution (w x h), and scene implementation
new Game(string title, int w, int h, IGame game);

// Initialize SDL, open the window, run the blocking main loop, and clean up on exit
game.Start();

// Manually release IGame, Drawer, active gamepads, and window handle
game.Dispose();
```

### Example
```csharp
// Create and start the game window
var game = new Game("My Game", 720, 480, new MyGame());
game.Start();

// Manual disposal (only needed if managing lifetime outside Start)
game.Dispose();
```
