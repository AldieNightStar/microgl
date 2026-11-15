namespace microgl;

using System;
using System.Numerics;
using SDL3;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Texture : IDisposable
{
    internal nint _surfacePtr;
    private readonly Dictionary<nint, nint> _textureCache;

    public nint NativeSurface => _surfacePtr;

    public Texture(nint surface)
    {
        if (surface == nint.Zero)
            throw new Exception("Surface points to NULL or didn't load correctly");

        _surfacePtr = surface;
        _textureCache = new Dictionary<nint, nint>();
    }

    public static Texture FromPNG(string filename) => new(SDL.LoadPNG(filename));
    public static Texture FromBMP(string filename) => new(SDL.LoadBMP(filename));
    public static Texture FromDraw(Drawer d, ushort w, ushort h, Action<Drawer> action) => d.GenerateTexture(w, h, action);

    public Vector2 Size
    {
        get
        {
            if (field == default)
            {
                var (w, h) = GetSurfaceSize(_surfacePtr);
                field = new Vector2(w, h);
            }
            return field;
        }
    }

    public void ClearCache()
    {
        if (_textureCache == null)
            return;

        foreach (nint texPtr in _textureCache.Values)
        {
            if (texPtr != nint.Zero)
                SDL.DestroyTexture(texPtr);
        }

        _textureCache.Clear();
    }

    private static nint _createNativeTexture(nint surface, nint rendererPtr)
    {
        nint texture = SDL.CreateTextureFromSurface(rendererPtr, surface);
        if (texture == nint.Zero)
            throw new Exception($"Texture initialization from surface wasn't successful: {SDL.GetError()}");

        SDL.SetTextureScaleMode(texture, SDL.ScaleMode.Nearest);
        return texture;
    }

    public void Dispose()
    {
        ClearCache();

        if (_surfacePtr != nint.Zero)
        {
            SDL.DestroySurface(_surfacePtr);
            _surfacePtr = IntPtr.Zero;
        }
    }

    public bool IsActive()
    {
        return _surfacePtr != IntPtr.Zero;
    }

    public static (int, int) GetSurfaceSize(nint surface)
    {
        var surf = Marshal.PtrToStructure<SDL.Surface>(surface);
        return (surf.Width, surf.Height);
    }

    internal nint GetTextureForRenderer(IRenderer renderer)
    {
        if (renderer == null || !renderer.IsValid())
            throw new ArgumentNullException(nameof(renderer), "Renderer is NULL or invalid");

        nint rendererPtr = renderer.GetNativePointer();
        if (rendererPtr == nint.Zero)
            throw new ArgumentException("Renderer points to NULL", nameof(renderer));

        if (_textureCache.TryGetValue(rendererPtr, out nint cachedTexture))
            return cachedTexture;

        // Create new texture if not present in cache
        nint newTexture = _createNativeTexture(_surfacePtr, rendererPtr);

        // Only cache textures for persistent GPU renderers; temporary software textures shouldn't store
        if (renderer is GPURenderer)
        {
            _textureCache[rendererPtr] = newTexture;
        }

        return newTexture;
    }

    internal static void FreeTextureForRenderer(nint texturePtr, IRenderer renderer)
    {
        if (renderer is GPURenderer) return;
        SDL.DestroyTexture(texturePtr);
    }
}
