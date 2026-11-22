# Drawer

The pen you draw with. Every `Init`, `Update` and `Draw` method receives one. Set a color or font, then call a `Draw...` method.

New to this? See [Tutorial 1: First window and drawing](../Tutorials/01-FirstWindow.md).

### Example
```csharp
public void Draw(Drawer d, float dt)
{
    d.Color = Color.CornflowerBlue;
    d.DrawRect(new Rectangle(20, 20, 200, 100));

    d.Color = Color.Orange;
    d.DrawRoundedRect(new Rectangle(240, 20, 200, 100), 12);

    d.DrawTexture(_logo, new Rectangle(20, 140, 64, 64));

    d.Font = _font;
    d.FontColor = Color.Gold;
    d.DrawText($"Score: {_score}", new Vector2(20f, 220f));

    _world.Draw(d);
}
```

### Notes
* **Draw state sticks**: `Color` (shapes and lines), `BlendColor` (texture tint), `Font`, `FontColor` and `Shader` stay set until you change them. `Game` calls `Reset()` before each `IGame.Draw`, which clears the screen to black and resets them.
* **Inside a `GameObject` the drawer is already moved**: in `OnDraw` and `OnUpdate`, `Transform` holds the object's position, rotation, scale and `CenterPoint`, so draw at `(0, 0)`. The previous state is restored when the object is done, so drawing after `group.Draw(d)` is in screen space again.
* **Child objects**: inside `OnDraw`, `DrawObject(child)` draws another object relative to the current one.
* **Text**: `DrawText` builds the text every call. For text shown every frame, a `Label` is faster.
* **Masks**: `BeginMask` clips drawing to the shape of a mask texture. See [Masking](Masking.md).

### Usage
```csharp
new Drawer(IRenderer renderer);

// Frame
drawer.Reset();        // clear the screen and reset Transform, colors, font and shader
drawer.Clear();        // clear the screen only

// State
drawer.Renderer;
drawer.ScreenSize;     // logical screen size given to Game
drawer.ScreenBounds;   // Rectangle(0, 0, ScreenSize)
drawer.Transform;      // applied to everything drawn, set by GameObject
drawer.Color;          // shapes and lines
drawer.BlendColor;     // texture tint, White = no tint
drawer.Font;
drawer.FontColor;
drawer.Shader;         // GPU effect, null = none

// Shapes and text
drawer.DrawRect(Rectangle rect);
drawer.DrawRoundedRect(Rectangle rect, int cornerRadius);   // radius is clamped to half the shortest side
drawer.DrawLine(Vector2 a, Vector2 b);
drawer.DrawText(string text, Vector2 pos);
drawer.DrawDebugText(string text, Vector2 pos);             // built-in font, no GameFont needed

// Textures
drawer.DrawTexture(Texture texture, Rectangle dstRect, Rectangle? srcRect = null);
drawer.DrawTexture(AtlasTexture texture, Rectangle rect);
drawer.DrawTexture(AnimatedTexture texture, Rectangle rect);   // current frame
drawer.DrawTexture(DynamicTexture texture, Rectangle rect);
drawer.DrawRoundedTexture(Texture texture, Rectangle dstRect, int cornerRadius);

// Objects, relative to the object currently being drawn
drawer.DrawObject(GameObject obj);
drawer.DrawObject(GameObject obj, Vector2 localPos);

// Masks: clip drawing inside dstRect (local coordinates) to a mask texture
using (drawer.BeginMask(Texture mask, Rectangle dstRect)) { ... }

drawer.Dispose();
```

### Example: child objects inside OnDraw
```csharp
protected override void OnDraw(Drawer d)
{
    d.DrawTexture(_body, new Rectangle(0, 0, 32, 32));

    d.DrawObject(_shadow);
    d.DrawObject(_hat, new Vector2(8f, -16f));
}
```

### Example: circular avatar
```csharp
public void Draw(Drawer d, float dt)
{
    using (d.BeginMask(_circleMask, new Rectangle(0, 0, 128, 128)))
        d.DrawTexture(_playerPhoto, new Rectangle(0, 0, 128, 128));
}
```
