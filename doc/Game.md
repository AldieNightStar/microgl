# Game

### Notes
* **Entry point and lifecycle manager** for a `microgl` application, implementing `IDisposable`.
* **Initializes SDL3 subsystems** (`Video` and `Gamepad`), creates a resizable window, and configures `Nearest` pixel-art scaling with `Letterbox` logical presentation.
* **Runs the main loop** by ticking `Time`, routing input events (`Mouse`, `Keyboard`, `Gamepad`), and calling `IGame.Init`, `IGame.Update`, and `IGame.Draw`.
* **Automatically cleans up** game resources, renderer, gamepads, and the native window when the loop exits.

### Usage
* `new Game(string title, int w, int h, IGame game)`
  Creates a game instance with the window `title`, logical resolution width `w` and height `h`, and an `IGame` implementation.
* `game.Start()`
  Initializes SDL, opens the window, runs the blocking main game loop until quit, and disposes resources on exit.
* `game.Dispose()`
  Frees the `IGame` instance, `Drawer`, active gamepads, and native window handle (called automatically at the end of `Start()`).

### Example
```csharp
// Create and start the game window
var game = new Game("My Game", 720, 480, new MyGame());
game.Start();

// Manual disposal (only needed if managing lifetime outside Start)
game.Dispose();
```
