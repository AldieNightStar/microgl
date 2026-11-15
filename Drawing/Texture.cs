using System.Diagnostics;
using SDL3;

namespace microgl;

public readonly struct Texture : IDisposable
{
    internal readonly nint _resource { get; init; }

    public Texture(nint resource)
    {
        if (resource == IntPtr.Zero) throw new Exception("Texture points to NULL or didn't load correctly");
        _resource = resource;
    }

    public static Texture FromPNG(Drawer d, string filename)
    {
        return new Texture(_toTexture(d, SDL.LoadPNG(filename)));
    }

    public static Texture FromBMP(Drawer d, string filename)
    {
        return new Texture(_toTexture(d, SDL.LoadBMP(filename)));
    }

    private static nint _toTexture(Drawer d, nint surface)
    {
        nint texture = SDL.CreateTextureFromSurface(d._renderer, surface);
        if (texture == IntPtr.Zero) throw new Exception("Texture initialization from surface wasn't successful");
        SDL.DestroySurface(surface);
        return texture;
    }

    public void Dispose() => SDL.DestroyTexture(_resource);
}