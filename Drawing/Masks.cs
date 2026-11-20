using System.Drawing;
using SDL3;

namespace microgl;

/// <summary>
/// Factory methods for common mask textures.
/// Each method returns a <see cref="Texture"/> whose alpha channel defines
/// the clip shape — pass it to <see cref="Drawer.BeginMask"/>.
/// </summary>
public static class Masks
{
    // === Factories ===========================================================

    /// <summary>Filled rectangle — every pixel inside is opaque.</summary>
    public static Texture Rectangle(int width, int height) =>
        Texture.FromDrawing((ushort)width, (ushort)height, d =>
        {
            d.Color = Color.White;
            d.DrawRect(new System.Drawing.Rectangle(0, 0, width, height));
        });

    /// <summary>Filled circle centred in a <paramref name="size"/> × <paramref name="size"/> canvas.</summary>
    public static Texture Circle(int size)
    {
        float cx = size / 2f;
        float cy = size / 2f;
        float radius = size / 2f;

        return Texture.FromDrawing((ushort)size, (ushort)size, d =>
        {
            d.Color = Color.White;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    if (MathF.Sqrt(dx * dx + dy * dy) <= radius)
                        d.DrawRect(new System.Drawing.Rectangle(x, y, 1, 1));
                }
        });
    }

    /// <summary>
    /// Filled rectangle with rounded corners of the given <paramref name="cornerRadius"/>.
    /// </summary>
    public static Texture RoundedRectangle(int width, int height, int cornerRadius)
    {
        int r = cornerRadius;

        return Texture.FromDrawing((ushort)width, (ushort)height, d =>
        {
            d.Color = Color.White;

            // Centre cross
            d.DrawRect(new System.Drawing.Rectangle(r, 0, width - r * 2, height));
            d.DrawRect(new System.Drawing.Rectangle(0, r, width, height - r * 2));

            // Four corner circles
            var corners = new[] { (r, r), (width - r, r), (r, height - r), (width - r, height - r) };
            foreach (var (cx, cy) in corners)
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        float dx = x - cx;
                        float dy = y - cy;
                        if (MathF.Sqrt(dx * dx + dy * dy) <= r)
                            d.DrawRect(new System.Drawing.Rectangle(x, y, 1, 1));
                    }
        });
    }

    /// <summary>
    /// Radial gradient mask: fully opaque at the centre, fading to fully transparent
    /// at the edges. Useful for soft vignettes, glows, and reveal effects.
    /// </summary>
    public static Texture Vignette(int width, int height, float strength = 1f)
    {
        float cx = width / 2f;
        float cy = height / 2f;
        float maxDistance = MathF.Sqrt(cx * cx + cy * cy);

        var surface = new Surface(width, height);

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float distance = MathF.Sqrt(dx * dx + dy * dy);
                float gradient = Math.Clamp(1f - distance / maxDistance, 0f, 1f);
                float brightness = 1f - strength * (1f - gradient);  // lerp(1, gradient, strength)
                byte alpha = (byte)(brightness * 255f);

                SDL.WriteSurfacePixel(surface, x, y, 255, 255, 255, alpha);
            }

        return new Texture(surface);
    }

    /// <summary>
    /// Converts a grayscale image into a mask: white → fully opaque, black → fully
    /// transparent, grey → semi-transparent. The brightness of each source pixel
    /// becomes the alpha of the corresponding mask pixel.
    /// </summary>
    public static Texture FromGrayscale(Texture grayscale)
    {
        var src = grayscale.Surface;
        var size = src.Size;
        int w = (int)size.X;
        int h = (int)size.Y;

        var dst = new Surface(w, h);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                SDL.ReadSurfacePixelFloat(src, x, y, out float r, out float g, out float b, out float _);

                // Luminance — simple average is fine for a greyscale source; handles tinted inputs too
                float brightness = (r + g + b) / 3f;
                byte alpha = (byte)(brightness * 255f);

                SDL.WriteSurfacePixel(dst, x, y, 255, 255, 255, alpha);
            }

        return new Texture(dst);
    }
}
