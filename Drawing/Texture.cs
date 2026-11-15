using System.Diagnostics;
using SDL3;

namespace microgl;

using System;
using SDL3;

public struct Texture : IDisposable
{
    internal readonly nint _surfacePtr;
    internal nint _texturePtr;
    private IRenderer _boundRenderer;

    public readonly nint NativeTexture => _texturePtr;
    public readonly nint NativeSurface => _surfacePtr;

    public Texture(nint surface)
    {
        if (surface == nint.Zero)
            throw new Exception("Surface points to NULL or didn't load correctly");

        _surfacePtr = surface;
        _texturePtr = nint.Zero;
        _boundRenderer = null;
    }

    public Texture(nint surface, IRenderer renderer) : this(surface)
    {
        RecreateTextureFor(renderer);
    }

    public static Texture FromPNG(string filename)
    {
        return new Texture(SDL.LoadPNG(filename));
    }

    public static Texture FromPNG(Drawer d, string filename)
    {
        return new Texture(SDL.LoadPNG(filename), d.Renderer);
    }

    public static Texture FromBMP(string filename)
    {
        return new Texture(SDL.LoadBMP(filename));
    }

    public static Texture FromBMP(Drawer d, string filename)
    {
        return new Texture(SDL.LoadBMP(filename), d.Renderer);
    }

    public void RecreateTextureFor(IRenderer renderer)
    {
        nint targetRendererPtr = renderer?.GetNativePointer() ?? IntPtr.Zero;
        nint boundRendererPtr = _boundRenderer?.GetNativePointer() ?? IntPtr.Zero;
        if (targetRendererPtr == nint.Zero)
            throw new Exception("Renderer points to NULL");

        // Skip recreation if this texture is already built for this renderer
        if (_texturePtr != nint.Zero && boundRendererPtr == targetRendererPtr)
            return;

        if (_texturePtr != nint.Zero)
        {
            SDL.DestroyTexture(_texturePtr);
            _texturePtr = nint.Zero;
        }

        nint texture = SDL.CreateTextureFromSurface(targetRendererPtr, _surfacePtr);
        if (texture == nint.Zero)
            throw new Exception($"Texture initialization from surface wasn't successful: {SDL.GetError()}");

        _texturePtr = texture;
        _boundRenderer = renderer;
    }

    // Call this if the underlying _surface pixels changed (e.g. after SoftwareRenderer.Present)
    public void ForceRecreateTextureFor(IRenderer renderer)
    {
        _boundRenderer = null;
        RecreateTextureFor(renderer);
    }

    public void Dispose()
    {
        if (_texturePtr != nint.Zero)
        {
            SDL.DestroyTexture(_texturePtr);
            _texturePtr = nint.Zero;
            _boundRenderer = null;
        }

        if (_surfacePtr != nint.Zero)
        {
            SDL.DestroySurface(_surfacePtr);
        }
    }
}
