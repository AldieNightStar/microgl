using System.Drawing;
using SDL3;

namespace microgl;

internal class TextureUtil
{
    public static nint ExtractRegion(nint sourceSurface, Rectangle region)
    {
        if (sourceSurface == IntPtr.Zero)
            throw new ArgumentException("Source surface cannot be NULL.", nameof(sourceSurface));

        if (region.Width <= 0 || region.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(region), "Region width and height must be greater than zero.");

        var (srcW, srcH) = Texture.GetSurfaceSize(sourceSurface);
        if (region.X < 0 || region.Y < 0 || region.Right > srcW || region.Bottom > srcH)
            throw new ArgumentOutOfRangeException(nameof(region), $"Region ({region}) exceeds source surface bounds ({srcW}x{srcH}).");

        nint destSurface = SDL.CreateSurface(region.Width, region.Height, SDL.PixelFormat.RGBA8888);
        if (destSurface == IntPtr.Zero)
            throw new Exception($"Failed to create destination surface: {SDL.GetError()}");

        var sdlSrc = new SDL.Rect { X = region.X, Y = region.Y, W = region.Width, H = region.Height };
        var sdlDst = new SDL.Rect { X = 0, Y = 0, W = region.Width, H = region.Height };

        // Save previous blend mode and disable blending to copy raw RGBA bytes intact
        SDL.GetSurfaceBlendMode(sourceSurface, out var previousBlendMode);
        SDL.SetSurfaceBlendMode(sourceSurface, SDL.BlendMode.None);

        try
        {
            if (!SDL.BlitSurface(sourceSurface, in sdlSrc, destSurface, in sdlDst))
            {
                SDL.DestroySurface(destSurface);
                throw new Exception($"Failed to blit surface region: {SDL.GetError()}");
            }
        }
        finally
        {
            // Restore the original blend mode on the source surface
            SDL.SetSurfaceBlendMode(sourceSurface, previousBlendMode);
        }

        return destSurface;
    }
}