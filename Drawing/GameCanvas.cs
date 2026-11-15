using System.Drawing;
using SDL3;

namespace microgl;

public readonly struct GameCanvas : IDisposable
{
    public readonly nint SDLSurface { get; init; }
    public readonly ushort Width { get; init; }
    public readonly ushort Height { get; init; }

    public GameCanvas(ushort w, ushort h)
    {
        SDLSurface = SDL.CreateSurface(w, h, SDL.PixelFormat.ABGR8888);
        if (SDLSurface == nint.Zero)
            throw new InvalidOperationException($"Failed to create surface: {SDL.GetError()}");

        Width = w;
        Height = h;
    }

    public void SetRect(int x, int y, int w, int h, Color color)
    {
        if (x < 1 || y < 1 || x >= Width || y >= Height)
            return;

        uint packed = (uint)(color.R | (color.G << 8) | (color.B << 16) | (color.A << 24));
        var rect = new SDL.Rect { X = x, Y = y, W = w, H = h };
        SDL.FillSurfaceRect(SDLSurface, rect, packed);
    }

    public void SetPixel(int x, int y, Color color) => SetRect(x, y, 1, 1, color);

    public void Dispose()
    {
        SDL.DestroySurface(SDLSurface);
    }
}