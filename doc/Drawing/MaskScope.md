# MaskScope

### Notes
* **Clips drawing to a mask shape** by redirecting rendering into an off-screen RGBA render target, compositing content onto the mask using an alpha-preserving blend, then blitting the result back to the screen.
* **Requires `GPURenderer`** — render targets are a GPU feature; constructing a `MaskScope` with a `SoftwareRenderer` throws `NotSupportedException`.
* **Open with `drawer.BeginMask(...)`** inside a `using` statement. Everything drawn while the scope is alive goes into the off-screen target. On `Dispose` the result is blitted to `dstRect`.
* **Draw content with `drawer.DrawTextureMasked`** inside the scope — it applies the inside-mask blend so only pixels where the mask was opaque survive.
* **The mask is fully invisible** — only its alpha channel is used to define the clip shape; its RGB is discarded. The final blit uses premultiplied alpha so zero-alpha pixels contribute nothing, preventing black or white fringing at the edges.

### What is a mask?

A mask is an ordinary `Texture` whose **alpha channel** defines the clipping shape:

* **Opaque (alpha = 255)** → content shows through.
* **Transparent (alpha = 0)** → content is cut away.

The RGB channels of the mask are ignored — only alpha matters. A white circle on a transparent background clips content to a circle. A bottle silhouette clips content to a bottle shape.

### Usage
```csharp
using (drawer.BeginMask(Texture mask, int width, int height, Rectangle dstRect))
{
    // Draw content in local (0, 0, width, height) space
    drawer.DrawTextureMasked(Texture texture, Rectangle dstRect, Rectangle? srcRect = null);
}
```

### Built-in masks

The `Masks` static class provides ready-made factory methods for common shapes.
Create them once in `Init` and reuse every frame.

```csharp
// Filled rectangle — every pixel inside is opaque
Texture mask = Masks.Rectangle(200, 120);

// Filled circle centred in a size × size canvas
Texture mask = Masks.Circle(128);

// Filled rectangle with rounded corners
Texture mask = Masks.RoundedRectangle(200, 120, cornerRadius: 16);
```

### Creating custom masks

For shapes not covered by `Masks`, use `Texture.FromDrawing` — whatever you draw becomes opaque in the mask, everything else stays transparent.

```csharp
// Bottle silhouette from a PNG (background must be fully transparent)
Texture bottleShape = Texture.FromPNG("bottle_mask.png");

// Hand-drawn shape
Texture custom = Texture.FromDrawing(128, 128, d =>
{
    d.Color = Color.White;
    // ... draw any shape here
});
```

For masks that change at runtime (e.g. a growing reveal circle) use `DynamicTexture` and call `UpdateTexture()` each frame before using the mask.

### Examples

**Circular player avatar**
```csharp
// In Init:
_circleMask  = Masks.Circle(128);
_playerPhoto = Texture.FromPNG("player.png");

// In OnDraw:
using (d.BeginMask(_circleMask, 128, 128, new Rectangle(20, 20, 128, 128)))
    d.DrawTextureMasked(_playerPhoto, new Rectangle(0, 0, 128, 128));
```

**Liquid bottle fill**
```csharp
// In Init:
_bottleShape = Texture.FromPNG("bottle_mask.png");

// In OnDraw:
int fillH = (int)(160 * _fillRatio);
using (d.BeginMask(_bottleShape, 64, 160, new Rectangle(40, 500, 64, 160)))
    d.DrawTextureMasked(_liquidTex, new Rectangle(0, 160 - fillH, 64, fillH));
```

**Rounded card**
```csharp
// In Init:
_cardMask = Masks.RoundedRectangle(300, 200, cornerRadius: 20);

// In OnDraw:
using (d.BeginMask(_cardMask, 300, 200, new Rectangle(100, 100, 300, 200)))
    d.DrawTextureMasked(_cardArt, new Rectangle(0, 0, 300, 200));
```

**Flashlight / fog-of-war reveal**
```csharp
// In Init:
_flashlightMask = Texture.FromPNG("flashlight_cone.png");

// In OnDraw:
var screen = new Rectangle((int)_playerX - 256, (int)_playerY - 256, 512, 512);
var mapSrc  = new Rectangle((int)_playerX - 256, (int)_playerY - 256, 512, 512);
using (d.BeginMask(_flashlightMask, 512, 512, screen))
    d.DrawTextureMasked(_hiddenMap, new Rectangle(0, 0, 512, 512), mapSrc);
```
