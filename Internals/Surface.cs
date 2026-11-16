using System.Numerics;
using System.Runtime.InteropServices;
using SDL3;

namespace microgl;

public readonly struct Surface : IDisposable
{
    public readonly nint Pointer { get; init; }

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

    public void Dispose()
    {
        SDL.DestroySurface(Pointer);
    }
    
    public static implicit operator nint(Surface s) => s.Pointer;
}