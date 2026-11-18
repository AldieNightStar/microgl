using System.Diagnostics;
using SDL3;

namespace microgl;

internal sealed class TextureCache : IDisposable
{
    private readonly nint _renderer;
    private readonly Dictionary<Texture, nint> _cache = new();
    private bool _disposed;

    public TextureCache(nint sdlRenderer)
    {
        if (sdlRenderer == IntPtr.Zero)
            throw new ArgumentException("Renderer pointer cannot be null.", nameof(sdlRenderer));

        _renderer = sdlRenderer;
        FreeingResourceBus.FreedTexture += OnTextureFreed;
    }

    public nint GetCachedSDLTexture(Texture texture)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_cache.TryGetValue(texture, out nint sdlTexture))
        {
            return sdlTexture;
        }

        sdlTexture = SDL.CreateTextureFromSurface(_renderer, texture.Surface.Pointer);
        if (sdlTexture == IntPtr.Zero)
        {
            throw new Exception($"Failed to create SDL texture: {SDL.GetError()}");
        }

        _cache[texture] = sdlTexture;
        return sdlTexture;
    }

    private void OnTextureFreed(Texture texture)
    {
        if (_disposed) return;
        if (_cache.Remove(texture, out nint deletedTexture) && deletedTexture != IntPtr.Zero)
        {
            SDL.DestroyTexture(deletedTexture);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        FreeingResourceBus.FreedTexture -= OnTextureFreed;

        foreach (var sdlTexture in _cache.Values)
        {
            if (sdlTexture != IntPtr.Zero)
            {
                SDL.DestroyTexture(sdlTexture);
            }
        }

        _cache.Clear();
    }
}
