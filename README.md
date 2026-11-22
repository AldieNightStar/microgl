# microgl

**microgl** is a small code-first 2D game framework for C#, built on **SDL3** (`SDL3-CS`). It gives you a game loop, hierarchical objects, drawing, masks, input, and tweens without an editor or engine overhead.

## Features

* **Game loop**: `IGame` lifecycle (`Init`, `Update`, `Draw`, `Dispose`), `Nearest` pixel-art scaling, `Letterbox` window resizing, and VSync on by default.
* **Drawing and textures**: GPU drawing through `Drawer` and `GPURenderer`, including textures baked on the GPU, with `Texture`, `AtlasTexture` sprite sheets, `DynamicTexture`, `Shader` effects, and TrueType fonts (`GameFont`).
* **Masked drawing**: `BeginMask` clips content to a mask texture (circles, rounded cards, liquid fills, soft vignettes). The `Masks` factory builds common masks.
* **Scene objects**: `GameObject` and `GameObjectGroup` with hierarchical position, rotation, scale and pivot.
* **Input**: `Mouse` (game coordinates), `Keyboard` (held keys and a direction helper), and `Gamepad` (per-controller ids, radial deadzones and rumble), and `TouchScreen` (per-finger down, move and up events).
* **Audio**: `Sound` loaded from WAV files or memory, with `Play`, `Pause`, `Stop`, seeking, looping, volume and speed. `SoundFX` generates sounds in code (tone, square, noise, laser, coin, explosion, bell).
* **Timing and animation**: `Time`, `GameTimer`, 31 `Easing` curves, `Tween`, `TweenPlanner`, `TweenSequence`, and 40 ready-made `TweenCollection` effects.

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
    private string _text = "Hello, microgl!";

    public static void Main()
    {
        new Game("microgl App", 720, 480, new MyGame()).Start();
    }

    public void Init(Drawer d)
    {
        _font = GameFont.FromTTF("Days.ttf", 20);
    }

    public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
    {
    }

    public void Draw(Drawer d, float dt)
    {
        d.Font = _font;
        d.FontColor = Color.White;
        d.DrawText(_text, d.ScreenSize / 2f - _font.MeasureText(_text) / 2f);
    }

    public void Dispose()
    {
        _font.Dispose();
    }
}
```

## How It Works

* **Your game is an `IGame`.** `Game` opens the window and calls `Init` once, then `Update` and `Draw` every frame, then `Dispose` when the window closes.
* **Objects follow the same pattern.** `GameObject` and `GameObjectGroup` have `Init`, `Update`, `Draw` and `Dispose`. Call them from the matching `IGame` method, or put objects in a `GameObjectGroup` and call the group only.
* **Coordinates** start at the top-left corner `(0, 0)`. `X` grows right and `Y` grows down. You always draw in the size you passed to `Game`, whatever the window size.
* **Frame time**: multiply speeds by `dt` (or `Time.DeltaTime` inside game objects) so movement is the same on every computer.
* **Load once, free once.** Load textures and fonts in `Init` and dispose them in `Dispose`, never every frame.

## Documentation Reference

### Core
* [Game](doc/Game.md): Application entry point, SDL3 initialization, and main game loop with optional `VSync`.
* [IGame](doc/IGame.md): Lifecycle interface implemented by your game or scene.

### Drawing
* [Drawer](doc/Drawing/Drawer.md): 2D drawing context for shapes, textures, text, objects, and masks.
* [Renderer](doc/Drawing/Renderer.md): GPU rendering backend (`GPURenderer`).
* [Texture](doc/Drawing/Texture.md): Image loading (`PNG`, `BMP`) and procedural texture creation.
* [AtlasTexture](doc/Drawing/AtlasTexture.md): Sprite-sheet sub-regions and grid slicing.
* [DynamicTexture](doc/Drawing/DynamicTexture.md): Offscreen texture redrawn on demand.
* [GameFont](doc/Drawing/GameFont.md): TrueType (`.ttf`) font loading and text measurement.
* [Transform](doc/Drawing/Transform.md): Immutable 2D affine transforms and coordinate conversion.
* [Shader](doc/Drawing/Shader.md): GPU fragment shader for effects.
* [Masking](doc/Drawing/Masking.md): Clip drawing to any shape with `BeginMask` and the `Masks` factory.

### GameObject
* [GameObject](doc/GameObject/GameObject.md): Base scene entity with hierarchical transforms and lifecycle hooks.
* [GameObjectGroup](doc/GameObject/GameObjectGroup.md): Container that runs and moves a group of objects together.

### Input
* [Mouse](doc/Input/Mouse.md): Cursor position in game coordinates and button, move and wheel events.
* [TouchScreen](doc/Input/TouchScreen.md): Active fingers and their positions, with down, move and up events.
* [Keyboard](doc/Input/Keyboard.md): Held keys, press and release events, and a direction from four keys.
* [Gamepad](doc/Input/Gamepad.md): Id-based controller input, connection events, deadzones and rumble.

### Audio
* [Sound](doc/Audio/Sound.md): Sound from WAV files or memory, with `Play`, `Pause`, `Stop`, seeking, looping, volume and speed.
* [SoundFX](doc/Audio/SoundFX.md): Procedural sound effects with settings records for waveform, pitch, duration and envelope.

### Timing
* [Time](doc/Timing/Time.md): Clamped frame `DeltaTime` and clocks.
* [GameTimer](doc/Timing/GameTimer.md): Single-shot and looping countdown timer.
* [Easing](doc/Timing/Easing.md): Interpolation curves for tweens.
* [ITween](doc/Timing/ITween.md): Common interface for tweens and sequences.
* [Tween](doc/Timing/Tween.md): Single value interpolator with easing and callbacks.
* [TweenSequence](doc/Timing/TweenSequence.md): Plays several tweens in order, with looping.
* [TweenPlanner](doc/Timing/TweenPlanner.md): Fluent builder for chains of tweens, waits and callbacks.
* [TweenCollection](doc/Timing/TweenCollection.md): 40 ready-made game-feel effects.
