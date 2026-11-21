# microgl

**microgl** is a lightweight, code-first 2D game framework for C# built on top of **SDL3** (`SDL3-CS`). It provides a clean, object-oriented API for 2D rendering, hierarchical transforms, input handling, timers, tweens, and UI layouts without heavy engine overhead.

## Features

* **Core Game Loop**: Simple `IGame` lifecycle (`Init`, `Update`, `Draw`, `Dispose`) with pixel-perfect `Nearest` scaling and automatic `Letterbox` window resizing.
* **2D Rendering & Textures**: Hardware (`GPURenderer`) and offscreen (`SoftwareRenderer`) drawing via `Drawer`, supporting primitives, `Transform` hierarchies, `Texture`, `AtlasTexture` sprite sheets, `AnimatedTexture`, `DynamicTexture`, and TrueType fonts (`GameFont`).
* **Masked Drawing**: `MaskScope` clips any content to an arbitrary mask texture shape (circular avatars, fog-of-war, liquid fills) with a single `using` block via `drawer.BeginMask(...)`. Built-in `Masks` factory covers circles, rectangles, rounded rectangles, and grayscale-to-mask conversion for soft edges and gradients.
* **Scene & UI System**: Hierarchical `GameObject` and `GameObjectGroup` entities, plus built-in `UIElement` widgets (`Label`, `Button`, `ClickableArea`) and row-based `UITable` layouts.
* **Unified Input**: Event-driven and polling APIs for `Mouse` (with automatic render-coordinate conversion), `Keyboard`, and `Gamepad` (with radial deadzones and rumble).
* **Timing & Animations**: High-resolution `Time`, countdown `GameTimer`, 31 `Easing` curves, `ITween` interfaces, fluent `TweenPlanner` and `TweenSequence` chaining, and ready-to-use `TweenCollection` game-feel presets.

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

## Documentation Reference

### Core
* [Game](doc/Game.md) — Application entry point, SDL3 initialization, and main game loop.
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
* [GameObjectGroup](doc/GameObject/GameObjectGroup.md) — Composite container for managing and transforming groups of objects.

#### UI
* [UIElement](doc/GameObject/UI/UIElement.md) — Base class for UI widgets with size clamping, origin, and bounds.
* [Label](doc/GameObject/UI/Label.md) — Cached TrueType text UI element.
* [Button](doc/GameObject/UI/Button.md) — Interactive rectangle or textured button with icon, label, and tween animations.
* [ClickableArea](doc/GameObject/UI/ClickableArea.md) — Invisible transform-aware mouse hitbox.
* [UITable](doc/GameObject/UI/UITable.md) — Row/column layout container over a `GameObjectGroup`, with padding, spacing, alignment, and a rounded color or texture background.

### Input
* [Mouse](doc/Input/Mouse.md) — Cursor coordinates (mapped to logical render space) and button events.
* [Keyboard](doc/Input/Keyboard.md) — Key press/release events and modifier key states (`Ctrl`, `Shift`, `Alt`).
* [Gamepad](doc/Input/Gamepad.md) — Controller hot-plugging, button/axis events, radial deadzones, and rumble.

### Timing
* [Time](doc/Timing/Time.md) — Clamped frame `DeltaTime` and high-resolution millisecond clock.
* [GameTimer](doc/Timing/GameTimer.md) — Single-shot and looping countdown timer.
* [Easing](doc/Timing/Easing.md) — Standard interpolation curves (`Linear`, `Sine`, `Quad`, `Cubic`, `Back`, `Elastic`, `Bounce`, etc.).
* [ITween](doc/Timing/ITween.md) — Base interface for controllable tweenable animations and sequences.
* [Tween](doc/Timing/Tween.md) — Value interpolator with easing, looping, and completion callbacks.
* [TweenSequence](doc/Timing/TweenSequence.md) — Plays multiple `ITween` instances sequentially or in parallel groups.
* [TweenPlanner](doc/Timing/TweenPlanner.md) — Fluent builder for chaining sequential tweens, delays, and callbacks.
* [TweenCollection](doc/Timing/TweenCollection.md) — Preset game-feel animations (shakes, flashes, squishes, jumps, and bumps).
