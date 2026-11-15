namespace microgl;

using System;
using System.Numerics;
using SDL3;

public struct Texture : IDisposable
{
    internal readonly nint _surfacePtr;
    internal readonly nint _texturePtr;
    private readonly IRenderer _boundRenderer;

    public nint NativeTexture => _texturePtr;
    public nint NativeSurface => _surfacePtr;
    public IRenderer BoundRenderer => _boundRenderer;

    public Texture(nint surface, IRenderer renderer)
    {
        if (surface == nint.Zero)
            throw new Exception("Surface points to NULL or didn't load correctly");

        _surfacePtr = surface;
        _boundRenderer = renderer ?? throw new ArgumentNullException("Renderer required to create new texture");
        if (renderer.GetNativePointer() == IntPtr.Zero) throw new ArgumentException("Renderer points to NULL");
        _texturePtr = _createNativeTexture(surface, renderer);
    }

    public static Texture FromPNG(IRenderer r, string filename) => new(SDL.LoadPNG(filename), r);
    public static Texture FromPNG(Drawer d, string filename) => new(SDL.LoadPNG(filename), d.Renderer);
    public static Texture FromBMP(IRenderer r, string filename) => new(SDL.LoadBMP(filename), r);
    public static Texture FromBMP(Drawer d, string filename) => new(SDL.LoadBMP(filename), d.Renderer);

    public Texture CreateFor(IRenderer renderer)
    {
        if (renderer == null)
            throw new ArgumentNullException("Renderer required to create new texture");

        nint duplicatedSurface = SDL.DuplicateSurface(_surfacePtr);
        if (duplicatedSurface == nint.Zero)
            throw new Exception($"Failed to duplicate surface: {SDL.GetError()}");

        return new Texture(duplicatedSurface, renderer);
    }

    private static nint _createNativeTexture(nint surface, IRenderer renderer)
    {
        nint rendererPtr = renderer.GetNativePointer();
        if (rendererPtr == nint.Zero)
            throw new Exception("Renderer points to NULL");

        nint texture = SDL.CreateTextureFromSurface(rendererPtr, surface);
        if (texture == nint.Zero)
            throw new Exception($"Texture initialization from surface wasn't successful: {SDL.GetError()}");

        return texture;
    }

    public void Dispose()
    {
        if (_texturePtr != nint.Zero)
            SDL.DestroyTexture(_texturePtr);

        if (_surfacePtr != nint.Zero)
            SDL.DestroySurface(_surfacePtr);
    }

    public Vector2 Size
    {
        get
        {
            SDL.GetTextureSize(_texturePtr, out var x, out var y);
            return new Vector2(x, y);
        }
    }
}
