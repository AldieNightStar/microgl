# Drawer

The pen you draw with. `Game` passes one to every `Init`, `Update` and `Draw` call. Set colors, a font or a shader, then call a `Draw` method.

### Example
```csharp
public void Draw(Drawer d, float dt)
{
    d.Color = Color.CornflowerBlue;
    d.DrawRect(new Rectangle(20, 20, 200, 100));

    d.Color = Color.Orange;
    d.DrawRectRound(new Rectangle(240, 20, 200, 100), 12);

    d.DrawTexture(_logo, new Rectangle(20, 140, 64, 64));

    d.Font = _font;
    d.FontColor = Color.Gold;
    d.DrawText($"Score: {_score}", new Vector2(20f, 220f));
}
```

### Notes
* **Reset before each frame**: `Game` calls `Reset()` before `IGame.Draw`. It clears the screen to black and restores `Transform`, `BlendColor`, `FontColor`, `Font` and `Shader` to their defaults. `Color` goes back to white.
* **Sticky state**: `Color` (shapes and lines), `BlendColor` (texture tint), `FontColor`, `Font` and `Shader` stay as you set them until you change them or the frame ends.
* **Inside a `GameObject`** the drawer is moved to the object: `Transform` holds its position, rotation, scale and `CenterPoint`, so draw at `(0, 0)`. The previous state is restored when the object is done.
* **Text**: `DrawText` needs `Font` set, or it throws. It builds the text on every call.
* **Masks**: `BeginMask` clips drawing to a mask texture. See [Masking](Masking.md).
* **Offscreen drawing**: create your own `Drawer` over a `SoftwareRenderer` to draw into a texture.

### Usage
```csharp
d.Reset();
d.Clear();

d.Renderer;
d.ScreenSize;
d.ScreenBounds;
d.Transform;
d.Color;
d.BlendColor;
d.FontColor;
d.Font;
d.Shader;

d.DrawRect(Rectangle rect);
d.DrawRectRound(Rectangle rect, int cornerRadius);
d.DrawLine(Vector2 a, Vector2 b);

d.DrawText(string text, Vector2 pos);
d.DrawDebugText(string text, Vector2 pos);

d.DrawTexture(Texture texture, Rectangle dstRect, Rectangle? srcRect = null);
d.DrawTextureRound(Texture texture, Rectangle dstRect, int cornerRadius);
d.DrawTexture(AtlasTexture texture, Rectangle rect);
d.DrawTexture(DynamicTexture texture, Rectangle rect);

d.DrawObject(GameObject obj);
d.DrawObject(GameObject obj, Vector2 localPos);

using (d.BeginMask(Texture mask, Rectangle dstRect))
{
    ...
}
```

`ScreenSize` is the logical size given to `Game`. `DrawDebugText` uses SDL's built-in font, so it needs no `GameFont`.

### Example: child objects inside OnDraw
```csharp
protected override void OnDraw(Drawer d)
{
    d.DrawTexture(_body, new Rectangle(0, 0, 32, 32));

    _shadow.Draw(this, d);
    _hat.Draw(this, d);
}
```
Children are drawn by their own `Draw` method, passing the parent. `Drawer` draws only shapes, textures and text.

### Example: circular avatar
```csharp
public void Draw(Drawer d, float dt)
{
    using (d.BeginMask(_circleMask, new Rectangle(0, 0, 128, 128)))
        d.DrawTexture(_playerPhoto, new Rectangle(0, 0, 128, 128));
}
```
