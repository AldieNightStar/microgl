# Masks

### Notes
* **Static factory class** that generates common mask textures ready for use with `MaskScope`.
* Each method returns a `Texture` whose alpha channel defines the clip shape — opaque where the shape is filled, transparent everywhere else.
* Create masks once in `Init` and reuse every frame — they are plain textures with no per-frame cost.

### Usage
```csharp
// Filled rectangle
Texture mask = Masks.Rectangle(int width, int height);

// Filled circle centred in a size × size canvas
Texture mask = Masks.Circle(int size);

// Filled rectangle with rounded corners
Texture mask = Masks.RoundedRectangle(int width, int height, int cornerRadius);

// Convert a grayscale image to a mask:
//   white  → fully opaque
//   black  → fully transparent
//   grey   → semi-transparent (brightness becomes alpha)
Texture mask = Masks.FromGrayscale(Texture grayscale);
```

### FromGrayscale

Useful for hand-painted or generated gradient masks — soft edges, vignettes, feathered fog-of-war, glow shapes, and so on. The source texture does not need to be strictly greyscale; for colour inputs the brightness is computed as the average of R, G, B.

```csharp
// Soft circular vignette from a painted gradient PNG
Texture vignetteMask = Masks.FromGrayscale(Texture.FromPNG("vignette.png"));

// Procedural gradient mask — bright centre fades to black at edges
Texture radialFade = Masks.FromGrayscale(Texture.FromDrawing(256, 256, d =>
{
    float cx = 128, cy = 128, maxDist = 128;
    for (int y = 0; y < 256; y++)
    for (int x = 0; x < 256; x++)
    {
        float dx         = x - cx;
        float dy         = y - cy;
        float distance   = MathF.Sqrt(dx * dx + dy * dy);
        float brightness = Math.Clamp(1f - distance / maxDist, 0f, 1f);
        byte  value      = (byte)(brightness * 255);
        d.Color = Color.FromArgb(255, value, value, value);
        d.DrawRect(new Rectangle(x, y, 1, 1));
    }
}));
```

### Example
```csharp
// In Init:
_avatarMask   = Masks.Circle(128);
_cardMask     = Masks.RoundedRectangle(300, 200, cornerRadius: 20);
_vignetteMask = Masks.FromGrayscale(Texture.FromPNG("vignette.png"));

// In OnDraw:
using (d.BeginMask(_avatarMask, 128, 128, new Rectangle(20, 20, 128, 128)))
    d.DrawTextureMasked(_playerPhoto, new Rectangle(0, 0, 128, 128));

using (d.BeginMask(_cardMask, 300, 200, new Rectangle(100, 100, 300, 200)))
    d.DrawTextureMasked(_cardArt, new Rectangle(0, 0, 300, 200));

using (d.BeginMask(_vignetteMask, 960, 720, new Rectangle(0, 0, 960, 720)))
    d.DrawTextureMasked(_sceneSnapshot, new Rectangle(0, 0, 960, 720));
```

See [MaskScope](MaskScope.md) for the full masking API and more examples.
