using SDL3;

namespace microgl;

public interface IRenderer : IDisposable
{
    nint GetNativePointer();
    void RenderPresent();
}

public readonly struct GPURenderer : IRenderer
{
    private nint _resource { get; init; }

    public GPURenderer(nint renderer)
    {
        _resource = renderer;
    }

    public void Dispose()
    {
        SDL.DestroyRenderer(_resource);
    }

    public nint GetNativePointer()
    {
        return _resource;
    }

    public void RenderPresent()
    {
        SDL.RenderPresent(_resource);
    }
}

public readonly struct SoftwareRenderer : IRenderer
{
    public nint SDLRenderer { get; init; }
    public nint SDLSurface { get; init; }
    public ushort Width { get; init; }
    public ushort Height { get; init; }

    public SoftwareRenderer(ushort w, ushort h)
    {
        Width = w;
        Height = h;
        SDLSurface = SDL.CreateSurface(w, h, SDL.PixelFormat.RGBA8888);
        if (SDLSurface == IntPtr.Zero) throw new Exception("Can't create SDL surface: " + SDL.GetError());

        SDLRenderer = SDL.CreateSoftwareRenderer(SDLSurface);
        if (SDLRenderer == IntPtr.Zero) throw new Exception("Can't create SDL Software Renderer: " + SDL.GetError());
    }

    public Texture ProduceTexture()
    {
        SDL.RenderPresent(SDLRenderer);
        nint sdlTexture = SDL.CreateTextureFromSurface(SDLRenderer, SDLSurface);
        if (sdlTexture == nint.Zero)
            throw new InvalidOperationException($"Failed to create texture: {SDL.GetError()}");
        return new Texture(sdlTexture);
    }

    public void Dispose()
    {
        SDL.DestroySurface(SDLSurface);
        SDL.DestroyRenderer(SDLRenderer);
    }

    public nint GetNativePointer()
    {
        return SDLRenderer;
    }

    public void RenderPresent()
    {
        SDL.RenderPresent(SDLRenderer);
    }
}