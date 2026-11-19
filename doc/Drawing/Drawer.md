# Drawer

### Notes
* **Core 2D rendering context** (`IDisposable`) wrapping an `IRenderer` to draw primitives, textures, text, and `GameObject` instances.
* **Applies `Transform` automatically** to all geometry, lines, textures, and text coordinates, allowing rotated and scaled rendering via `SDL.RenderGeometry`.
* **Tracks stateful styling and viewport dimensions** through `ScreenSize` (the logical screen size configured when creating `Game`), `Color` (primitive fill/line color), `BlendColor` (texture tint), `Font`, and `FontColor`, resettable per frame with `Reset()`.

### Usage
```csharp
// Create a Drawer backed by an IRenderer (GPURenderer or SoftwareRenderer)
new Drawer(IRenderer renderer);

// Frame state management: reset transform/colors/font or clear the screen to black
drawer.Reset();
drawer.Clear();

// Rendering state & viewport properties
drawer.Renderer;
drawer.ScreenSize;
drawer.Transform;
drawer.Color;
drawer.BlendColor;
drawer.Font;
drawer.FontColor;

// Draw a Texture (with optional source region), AtlasTexture, or DynamicTexture
drawer.DrawTexture(Texture texture, Rectangle dstRect, Rectangle? srcRect = null);
drawer.DrawTexture(AtlasTexture texture, Rectangle rect);
drawer.DrawTexture(DynamicTexture texture, Rectangle rect);

// Draw a GameObject using current Transform or at an offset position
drawer.DrawObject(GameObject obj);
drawer.DrawObject(GameObject obj, Vector2 pos);

// Draw primitives and text (Font + FontColor for TTF text, or built-in SDL debug text)
drawer.DrawRect(Rectangle rect);
drawer.DrawLine(Vector2 vec1, Vector2 vec2);
drawer.DrawText(string text, Vector2 vec);
drawer.DrawDebugText(string text, Vector2 vec);

// Dispose the underlying IRenderer
drawer.Dispose();
```

### Example
```csharp
drawer.Reset();

// Centering using logical screen size
Vector2 center = drawer.ScreenSize / 2f;

// Primitives
drawer.Color = Color.CornflowerBlue;
drawer.DrawRect(new Rectangle(20, 20, 100, 60));
drawer.DrawLine(new Vector2(20f, 100f), new Vector2(120f, 100f));

// Textures & objects
drawer.BlendColor = Color.White;
drawer.DrawTexture(sprite, new Rectangle(150, 20, 64, 64));
drawer.DrawObject(player, center);

// TTF and debug text
drawer.Font = font;
drawer.FontColor = Color.Gold;
drawer.DrawText("Score: 100", new Vector2(20f, 120f));
drawer.DrawDebugText("FPS: 60", new Vector2(20f, 150f));
```
