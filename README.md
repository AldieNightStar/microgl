# microgl

A lightweight, high-performance 2D graphics engine wrapper for **SDL3** in C#. 

`microgl` is designed to simplify the creation of pixel-art and retro-style games by providing a clean, modern interface for SDL3 while handling the complexities of window management, pixel-perfect scaling, and event loops.

## ✨ Key Features

*   **Pixel-Perfect Rendering**: Built-in support for `Nearest` texture scaling and automatic letterboxing to maintain your game's aspect ratio without distortion.
*   **Simplified Game Loop**: Implements a clean `IGame` interface, separating initialization, update (logic), and draw (rendering) phases.
*   **Modern C# & .NET**: Built for modern development using **.NET 11** and high-level abstractions over SDL3.
*   **Event Handling**: Integrated support for keyboard and mouse events directly within the game loop.
*   **Hardware Accelerated**: Leverages SDL3's GPU-based rendering for efficient 2D performance.

## 🚀 Getting Started

### Prerequisites

*   **.NET 11 SDK** (or higher)
*   **SDL3 Libraries**: Ensure the SDL3 shared libraries (`.dll`, `.so`, or `.dylib`) are available in your output directory or system path.

### Quick Start Example

To create a game with `microgl`, simply implement the `IGame` interface:

```csharp
using microgl;
using SDL3;

public class MyGame : IGame
{
    private Drawer _drawer;

    public void Init(Drawer d) 
    {
        _drawer = d;
        // Initialize your game assets here
    }

    public void Update(Drawer d, ref SDL.Event ev, float dt)
    {
        // Handle input and logic
    }

    public void Draw(Drawer d, float dt)
    {
        // Render your scene
    }

    public void Dispose() 
    {
        // Cleanup assets
    }
}

// To run:
var game = new MyGame();
var engine = new Game("My Awesome Game", 800, 600, game);
engine.Start();
```

## 📂 Project Structure

*   `Drawing/`: The core engine implementation (the `microgl` namespace).
*   `IGame.cs`: The interface for implementing custom game logic.
*   `Game.cs`: The main engine entry point and window manager.
*   `Drawer.cs`: The drawing abstraction layer for rendering primitives and textures.

## 🛠 Building & Running

Ensure you have the .NET SDK installed, then run:

```bash
dotnet build
dotnet run --project microgl
```

## License

[Specify your license here, e.g., MIT]
