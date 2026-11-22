using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using SDL3;

namespace microgl;

public sealed class Surface : IDisposable
{
    public nint Pointer { get; private set; }

    public Vector2 Size => new Vector2(
        Marshal.ReadInt32(Pointer, 8),
        Marshal.ReadInt32(Pointer, 12)
    );

    public Surface(int w, int h)
    {
        Pointer = SDL.CreateSurface(w, h, SDL.PixelFormat.RGBA8888);
        if (Pointer == IntPtr.Zero)
        {
            throw new Exception("Can't create SDL Surface: " + SDL.GetError());
        }
    }

    internal Surface(nint surface)
    {
        if (surface == IntPtr.Zero) throw new Exception("Got NULL Surface");
        Pointer = surface;
    }

    public static Surface Clone(Surface surface)
    {
        return new Surface(SDL.DuplicateSurface(surface.Pointer));
    }

    public void WritePixel(int x, int y, byte r, byte g, byte b, byte a)
        => SDL.WriteSurfacePixel(this, x, y, r, g, b, a);

    public void WritePixel(int x, int y, Color color)
        => SDL.WriteSurfacePixel(this, x, y, color.R, color.G, color.B, color.A);

    public void Dispose()
    {
        if (Pointer == IntPtr.Zero) return;

        SDL.DestroySurface(Pointer);
        Pointer = IntPtr.Zero;
    }

    public static implicit operator nint(Surface s) => s.Pointer;
}