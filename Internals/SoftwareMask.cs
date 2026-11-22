using System.Drawing;
using SDL3;

namespace microgl;

internal static class SoftwareMask
{
    private const float CHANNEL_MAX = 255f;

    internal static Texture Apply(Surface content, Texture mask, Rectangle dstRect)
    {
        var size = content.Size;
        int w = (int)size.X;
        int h = (int)size.Y;
        var result = new Surface(w, h);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                _writeMasked(result, content, mask.Surface, x, y, dstRect);

        return new Texture(result);
    }

    private static void _writeMasked(Surface result, Surface content, Surface mask, int x, int y, Rectangle dstRect)
    {
        SDL.ReadSurfacePixelFloat(content, x, y, out float r, out float g, out float b, out float a);
        float maskAlpha = _maskAlphaAt(mask, x - dstRect.X, y - dstRect.Y);
        result.WritePixel(x, y, _toByte(r), _toByte(g), _toByte(b), _toByte(a * maskAlpha));
    }

    private static float _maskAlphaAt(Surface mask, int x, int y)
    {
        var size = mask.Size;
        if (x < 0 || y < 0 || x >= size.X || y >= size.Y) return 0f;

        SDL.ReadSurfacePixelFloat(mask, x, y, out _, out _, out _, out float a);
        return a;
    }

    private static byte _toByte(float channel) => (byte)Math.Clamp(channel * CHANNEL_MAX, 0f, CHANNEL_MAX);
}
