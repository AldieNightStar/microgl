namespace microgl;

using System;
using System.Numerics;
using SDL3;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public struct Texture : IDisposable
{
    internal readonly nint _surfacePtr;
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

    public nint GetTextureForRenderer(IRenderer renderer)
    {
        if (renderer == null || !renderer.IsValid())
            throw new ArgumentNullException("Renderer is NULL or invalid");

        nint rendererPtr = renderer.GetNativePointer();
        if (rendererPtr == nint.Zero)
            throw new ArgumentException("Renderer points to NULL");

        if (_textureCache.TryGetValue(rendererPtr, out nint cachedTexture))
            return cachedTexture;

        nint newTexture = _createNativeTexture(_surfacePtr, rendererPtr);
        _textureCache[rendererPtr] = newTexture;
        return newTexture;
    }

    public Vector2 Size
    {
        get
        {
            if (field == default) {
                // Reading few bytes from surface
                var w = Marshal.ReadInt32(_surfacePtr, 8);
                var h = Marshal.ReadInt32(_surfacePtr, 12);
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

        return texture;
    }

    public void Dispose()
    {
        ClearCache();

        if (_surfacePtr != nint.Zero)
            SDL.DestroySurface(_surfacePtr);
    }
}
