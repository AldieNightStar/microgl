# Masking

Clip drawing to any shape using `MaskScope` and a mask `Texture`.

### Notes
* **`ref struct`** — stack-allocated, no GC pressure. Always use with `using`.
* **`DrawTexture` works as normal** inside the scope — no separate method needed.
* **Partial draws are truly invisible** — undrawn areas stay transparent; the background shows through with no black, white, or color bleed.
* **Requires `GPURenderer`** — constructing a `MaskScope` with `SoftwareRenderer` throws `NotSupportedException`.
* Create mask textures once in `Init` and reuse every frame.

### Usage

```csharp
using (drawer.BeginMask(mask, width, height, dstRect))
{
    drawer.DrawTexture(texture, new Rectangle(0, 0, width, height));
}
```

### Built-in masks (`Masks`)

```csharp
Texture mask = Masks.Rectangle(200, 120);
Texture mask = Masks.Circle(128);
Texture mask = Masks.RoundedRectangle(200, 120, cornerRadius: 16);

// Radial gradient — fully opaque at centre, fades to transparent at edges.
// strength: 0 = no vignette (fully opaque), 1 = full fade. Default is 0.8f
Texture mask = Masks.Vignette(960, 720, strength: 0.8f);

// Grayscale image → mask: white = opaque, black = transparent, grey = semi.
// disposeOlder: pass true to dispose the source texture after conversion.
Texture mask = Masks.FromGrayscale(Texture.FromPNG("vignette.png"), disposeOlder: true);
```

### Custom masks

```csharp
// From a PNG with a transparent background
Texture mask = Texture.FromPNG("bottle_mask.png");

// Drawn procedurally — use Surface.WritePixel(x, y, Color) for per-pixel control
Texture mask = Texture.FromDrawing(128, 128, d =>
{
    d.Color = Color.White;
    // draw any shape here
});
```

For masks that change each frame, use `DynamicTexture` and call `UpdateTexture()` before `BeginMask`.

### Examples

**Circle avatar**
```csharp
_circleMask = Masks.Circle(128);

using (d.BeginMask(_circleMask, 128, 128, new Rectangle(20, 20, 128, 128)))
    d.DrawTexture(_playerPhoto, new Rectangle(0, 0, 128, 128));
```

**Rounded card**
```csharp
_cardMask = Masks.RoundedRectangle(300, 200, cornerRadius: 20);

using (d.BeginMask(_cardMask, 300, 200, new Rectangle(100, 100, 300, 200)))
    d.DrawTexture(_cardArt, new Rectangle(0, 0, 300, 200));
```

**Liquid bottle fill**
```csharp
_bottleMask = Texture.FromPNG("bottle_mask.png");

int fillH = (int)(160 * _fillRatio);
using (d.BeginMask(_bottleMask, 64, 160, new Rectangle(40, 500, 64, 160)))
    d.DrawTexture(_liquidTex, new Rectangle(0, 160 - fillH, 64, fillH));
```

**Soft vignette**
```csharp
_vignetteMask = Masks.Vignette(960, 720, strength: 0.8f);

using (d.BeginMask(_vignetteMask, 960, 720, new Rectangle(0, 0, 960, 720)))
    d.DrawTexture(_scene, new Rectangle(0, 0, 960, 720));
```
