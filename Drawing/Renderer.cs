using SDL3;

namespace microgl;

public interface IRenderer : IDisposable
{
    nint GetNativePointer();
    void RenderPresent();
    bool IsValid();
}

public struct GPURenderer : IRenderer
{
    private nint _rendererPtr;

    public GPURenderer(nint renderer)
    {
        _rendererPtr = renderer;
        if (_rendererPtr == IntPtr.Zero) throw new ArgumentException("GPU Renderer is not initialized well");
    }

    public void Dispose()
    {
        SDL.DestroyRenderer(_rendererPtr);
        _rendererPtr = IntPtr.Zero;
    }

    public nint GetNativePointer()
    {
        return _rendererPtr;
    }

    public void RenderPresent()
    {
        SDL.RenderPresent(_rendererPtr);
    }

    public bool IsValid()
    {
        return _rendererPtr != IntPtr.Zero;
    }
}

public struct SoftwareRenderer : IRenderer
{
    public nint SDLRenderer { get; private set; }
    public readonly nint SDLSurface { get; init; }
    public readonly ushort Width { get; init; }
    public readonly ushort Height { get; init; }

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
        var tex = new Texture(SDL.DuplicateSurface(SDLSurface));
        SDL.RenderClear(SDLRenderer);
        return tex;
    }

    public void Dispose()
    {
        if (SDLRenderer == IntPtr.Zero) return;

        SDL.DestroySurface(SDLSurface);
        SDL.DestroyRenderer(SDLRenderer);
        SDLRenderer = IntPtr.Zero;
    }

    public nint GetNativePointer()
    {
        return SDLRenderer;
    }

    public void RenderPresent()
    {
        SDL.RenderPresent(SDLRenderer);
    }

    public bool IsValid()
    {
        return SDLRenderer != IntPtr.Zero;
    }
}