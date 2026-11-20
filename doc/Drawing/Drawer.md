# Drawer

### Notes
* **Core 2D rendering context** (`IDisposable`) wrapping an `IRenderer` to draw primitives, textures, text, and `GameObject` instances.
* **Applies `Transform` automatically** to all geometry — the transform includes the current object's `CenterPoint` pivot offset and is set by `GameObject` before calling `OnDraw`/`OnUpdate`.
* **`WorldTransform`** holds the current object's pure world transform (no pivot shift) and is used by `DrawObject` as the parent transform for any child objects spawned inside `OnDraw`. It is managed automatically by `DrawerScope` inside `GameObject` — do not set it manually.
* **Tracks stateful styling** through `Color`, `BlendColor`, `Font`, and `FontColor`, all reset per frame by `Reset()`.

### Usage
```csharp
// Create a Drawer backed by an IRenderer
new Drawer(IRenderer renderer);

// Frame lifecycle: call Reset() once at the start of each Draw pass
drawer.Reset();   // clears screen, resets Transform, WorldTransform, colors, font, shader
drawer.Clear();   // clears screen only

// State properties
drawer.Renderer;
drawer.ScreenSize;
drawer.Transform;      // render transform (set by GameObject, includes pivot offset)
drawer.Color;          // fill/line color
drawer.BlendColor;     // texture tint
drawer.Font;
drawer.FontColor;
drawer.Shader;

// Draw a GameObject as a child of the current object (inherits WorldTransform)
drawer.DrawObject(GameObject obj);

// Draw a GameObject at a local offset relative to the current object
drawer.DrawObject(GameObject obj, Vector2 localPos);

// Draw textures
drawer.DrawTexture(Texture texture, Rectangle dstRect, Rectangle? srcRect = null);
drawer.DrawTexture(AtlasTexture texture, Rectangle rect);
drawer.DrawTexture(DynamicTexture texture, Rectangle rect);

// Draw primitives and text
drawer.DrawRect(Rectangle rect);
drawer.DrawLine(Vector2 a, Vector2 b);
drawer.DrawText(string text, Vector2 pos);
drawer.DrawDebugText(string text, Vector2 pos);

drawer.Dispose();
```

### Example
```csharp
// In IGame.Draw:
drawer.Reset();

// Background
drawer.Color = Color.CornflowerBlue;
drawer.DrawRect(new Rectangle(0, 0, 960, 720));

// HUD text drawn in screen space — safe after Reset()
drawer.DrawDebugText($"Score: {score}", new Vector2(24f, 20f));

// Draw the scene — objects handle their own transforms internally
sceneGroup.Draw(drawer);

// After sceneGroup.Draw returns, drawer.Transform is restored to Identity
// so any subsequent draws here are in screen space again
drawer.Color = Color.White;
drawer.DrawDebugText("GAME OVER", new Vector2(400f, 360f));
```

### DrawObject inside OnDraw
```csharp
protected override void OnDraw(Drawer d)
{
    // Draws childObj as a child — it inherits this object's WorldTransform
    d.DrawObject(childObj);

    // Draws at a local offset relative to this object
    d.DrawObject(icon, new Vector2(8f, -16f));
}
```
