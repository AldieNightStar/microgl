using System.Drawing;
using SDL3;

namespace microgl;

internal class TextureUtil
{
    public static nint ExtractRegion(Surface srcSurface, Rectangle region)
    {
        if (region.Width <= 0 || region.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(region), "Region width and height must be greater than zero.");

        var srcSize = srcSurface.Size;
        if (region.X < 0 || region.Y < 0 || region.Right > srcSize.X || region.Bottom > srcSize.Y)
            throw new ArgumentOutOfRangeException(nameof(region), $"Region ({region}) exceeds source surface bounds ({srcSize.X}x{srcSize.Y}).");

        Surface destSurface = new Surface(region.Width, region.Height);

        var sdlSrc = new SDL.Rect { X = region.X, Y = region.Y, W = region.Width, H = region.Height };
        var sdlDst = new SDL.Rect { X = 0, Y = 0, W = region.Width, H = region.Height };

        // Save previous blend mode and disable blending to copy raw RGBA bytes intact
        SDL.GetSurfaceBlendMode(srcSurface, out var previousBlendMode);
        SDL.SetSurfaceBlendMode(srcSurface, SDL.BlendMode.None);

        try
        {
            if (!SDL.BlitSurface(srcSurface, in sdlSrc, destSurface, in sdlDst))
            {
                SDL.DestroySurface(destSurface);
                throw new Exception($"Failed to blit surface region: {SDL.GetError()}");
            }
        }
        finally
        {
            // Restore the original blend mode on the source surface
            SDL.SetSurfaceBlendMode(srcSurface, previousBlendMode);
        }

        return destSurface;
    }
}