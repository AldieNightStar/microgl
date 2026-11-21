# microgl

**microgl** is a lightweight, code-first 2D game framework for C# built on top of **SDL3** (`SDL3-CS`). It provides a clean, object-oriented API for 2D rendering, hierarchical transforms, input handling, timers, tweens, and UI layouts without heavy engine overhead.

## Features

* **Core Game Loop**: Simple `IGame` lifecycle (`Init`, `Update`, `Draw`, `Dispose`) with pixel-perfect `Nearest` scaling, automatic `Letterbox` window resizing, VSync on by default, and allocation-free per-frame UI layout.
* **2D Rendering & Textures**: Hardware (`GPURenderer`) and offscreen (`SoftwareRenderer`) drawing via `Drawer`, supporting primitives, `Transform` hierarchies, `Texture`, `AtlasTexture` sprite sheets, `AnimatedTexture`, `DynamicTexture`, and TrueType fonts (`GameFont`).
* **Masked Drawing**: `MaskScope` clips any content to an arbitrary mask texture shape (circular avatars, fog-of-war, liquid fills) with a single `using` block via `drawer.BeginMask(...)`. Built-in `Masks` factory covers circles, rectangles, rounded rectangles, and grayscale-to-mask conversion for soft edges and gradients.
* **Scene & UI System**: Hierarchical `GameObject` and `GameObjectGroup` entities, plus built-in `UIElement` widgets (`Label`, `Button`, `ClickableArea`), row-based `UITable` layouts, and `PagedList` paged containers.
* **Unified Input**: Event-driven and polling APIs for `Mouse` (with automatic render-coordinate conversion), `Keyboard` (held keys and a WASD/arrows direction helper), and `Gamepad` (with radial deadzones and rumble).
* **Timing & Animations**: High-resolution `Time`, countdown `GameTimer`, 31 `Easing` curves, `ITween` interfaces, fluent `TweenPlanner` and `TweenSequence` chaining, and ready-to-use `TweenCollection` game-feel presets.

## Getting Started

1. Create a console app and reference microgl:
   ```
   dotnet new console -n MyGame
   cd MyGame
   dotnet add reference ../microgl/microgl.csproj
   ```
2. `SDL3-CS` only contains the C# bindings. Put the native `SDL3` and `SDL3_ttf` libraries next to your executable (for example `SDL3.dll` and `SDL3_ttf.dll` on Windows).
3. Copy your assets (fonts, images) to the build output, for example with `<None Include="Assets/**" CopyToOutputDirectory="PreserveNewest" />` in your `.csproj`.
4. Paste the Quick Start below into `Program.cs` and run `dotnet run`.

## Quick Start

```csharp
using System.Drawing;
using System.Numerics;
using SDL3;
using microgl;

public class MyGame : IGame
{
    private GameFont _font = null!;
    private Label _label = null!;

    public static void Main()
    {
        var game = new Game("microgl App", 720, 480, new MyGame());
        game.Start();
    }

    public void Init(Drawer d)
    {
        _font = GameFont.FromTTF("Days.ttf", 20);
        _label = new Label(_font, d.ScreenSize / 2f, "Hello, microgl!", Color.White, centerOrigin: true);
        _label.Init(d);
    }

    public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
    {
        _label.Update(d);
    }

    public void Draw(Drawer d, float dt)
    {
        _label.Draw(d);
    }

    public void Dispose()
    {
        _label.Dispose();
        _font.Dispose();
    }
}
```

## How It Works

* **Your game is an `IGame`.** `Game` opens the window and calls `Init` once, then `Update` and `Draw` every frame, then `Dispose` when the window closes.
* **Everything else follows the same pattern.** Game objects, UI elements and groups have `Init`, `Update`, `Draw` and `Dispose`. Call them from the matching `IGame` method, or put objects in a `GameObjectGroup` and call them on the group only.
* **Coordinates** start at the top-left corner `(0, 0)`, `X` grows right and `Y` grows down. You always draw in the size you passed to `Game`, whatever the window size.
* **Frame time**: multiply speeds by `dt` (or `Time.DeltaTime` inside game objects) so movement is the same on every computer.
* **Load once, free once.** Load textures and fonts in `Init` and dispose them in `Dispose`, never every frame.

## Documentation Reference

### Core
* [Game](doc/Game.md) — Application entry point, SDL3 initialization, and main game loop with optional `VSync`.
* [IGame](doc/IGame.md) — Core lifecycle interface implemented by your game or scene.

### Drawing
* [Drawer](doc/Drawing/Drawer.md) — 2D rendering context for shapes, textures, text, and game objects.
* [Renderer](doc/Drawing/Renderer.md) — Hardware (`GPURenderer`) and offscreen (`SoftwareRenderer`) rendering backends.
* [Texture](doc/Drawing/Texture.md) — 2D image loading (`PNG`/`BMP`) and procedural texture creation.
* [AtlasTexture](doc/Drawing/AtlasTexture.md) — Lightweight sprite-sheet sub-regions and grid slicing.
* [AnimatedTexture](doc/Drawing/AnimatedTexture.md) — Frame-by-frame animation over equally sized `AtlasTexture` frames.
* [DynamicTexture](doc/Drawing/DynamicTexture.md) — Mutable offscreen texture updated on demand.
* [GameFont](doc/Drawing/GameFont.md) — TrueType (`.ttf`) font loading and text measurement.
* [Transform](doc/Drawing/Transform.md) — Immutable 2D affine matrix transformations and coordinate conversions.
* [Shader](doc/Drawing/Shader.md) — GPU fragment shader and render-state wrapper for effects.
* [Masking](doc/Drawing/Masking.md) — Clip drawing to any shape with `MaskScope` and the `Masks` factory (circles, rectangles, rounded rectangles, grayscale gradients).

### GameObject
* [GameObject](doc/GameObject/GameObject.md) — Base scene entity with hierarchical transforms and lifecycle hooks.
* [GameObjectGroup](doc/GameObject/GameObjectGroup.md) — Composite container for managing and transforming groups of objects, with an `OnChildrenChanged` event and allocation-free `CollectChildren<T>`.

### Input
* [Mouse](doc/Input/Mouse.md) — Cursor coordinates (mapped to logical render space) and button events.
* [Keyboard](doc/Input/Keyboard.md) — Held, pressed and released keys, a direction from arrows or WASD (`GetVectorFromKeys`), modifier states and key events.
* [Gamepad](doc/Input/Gamepad.md) — Controller hot-plugging, button/axis events, radial deadzones, and rumble.

### Timing
* [Time](doc/Timing/Time.md) — Clamped frame `DeltaTime` and high-resolution millisecond clock.
* [GameTimer](doc/Timing/GameTimer.md) — Single-shot and looping countdown timer.
* [Easing](doc/Timing/Easing.md) — Standard interpolation curves (`Linear`, `Sine`, `Quad`, `Cubic`, `Back`, `Elastic`, `Bounce`, etc.).
* [ITween](doc/Timing/ITween.md) — Base interface for controllable tweenable animations and sequences.
* [Tween](doc/Timing/Tween.md) — Single value interpolator with easing and completion callbacks.
* [TweenSequence](doc/Timing/TweenSequence.md) — Plays multiple `ITween` instances in order, loops, and stops when a target object dies.
* [TweenPlanner](doc/Timing/TweenPlanner.md) — Fluent builder for chaining tweens, delays, and callbacks, with `For(obj)` object binding.
* [TweenCollection](doc/Timing/TweenCollection.md) — About 40 ready-made game-feel effects (shakes, flashes, squishes, jumps, pops, and more).
