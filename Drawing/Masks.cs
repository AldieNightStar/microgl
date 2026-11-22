using System.Drawing;
using SDL3;

namespace microgl;

public static class Masks
{
    public static Texture Rectangle(int width, int height) =>
        Texture.FromDrawing((ushort)width, (ushort)height, d =>
        {
            d.Color = Color.White;
            d.DrawRect(new Rectangle(0, 0, width, height));
        });

    public static Texture Circle(int size)
    {
        float center = size / 2f;
        float radius = size / 2f;

        return _fromCoverage(size, size, (x, y) =>
        {
            float dx = x - center;
            float dy = y - center;
            return dx * dx + dy * dy <= radius * radius;
        });
    }

    public static Texture RoundedRectangle(int width, int height, int cornerRadius) =>
        _fromCoverage(width, height, (x, y) => _insideRoundedRect(x, y, width, height, cornerRadius));

    private static bool _insideRoundedRect(int x, int y, int width, int height, int r)
    {
        if ((x >= r && x < width - r) || (y >= r && y < height - r)) return true;

        float cornerX = x < r ? r : width - r;
        float cornerY = y < r ? r : height - r;
        float dx = x - cornerX;
        float dy = y - cornerY;
        return dx * dx + dy * dy <= r * r;
    }

    private static Texture _fromCoverage(int width, int height, Func<int, int, bool> covered)
    {
        var surface = new Surface(width, height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (covered(x, y)) surface.WritePixel(x, y, 255, 255, 255, 255);
                else surface.WritePixel(x, y, 0, 0, 0, 0);
            }

        return new Texture(surface);
    }

    public static Texture Vignette(int width, int height, float strength = 0.8f)
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

    public static Texture FromGrayscale(Texture grayscale, bool disposeOlder = false)
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
                
                dst.WritePixel(x, y, 255, 255, 255, alpha);
            }

        if (disposeOlder) grayscale.Dispose();

        return new Texture(dst);
    }
}
