using System.Drawing;
using SDL3;

namespace microgl;

public interface IRenderer : IDisposable
{
    nint GetNativePointer();
    void RenderPresent();
    bool IsValid();
}

public sealed class GPURenderer : IRenderer
{
    private nint _rendererPtr;

    public GPURenderer(nint renderer)
    {
        _rendererPtr = renderer;
        if (_rendererPtr == IntPtr.Zero) throw new ArgumentException("GPU Renderer is not initialized well");
    }

    public void Dispose()
    {
        if (_rendererPtr != IntPtr.Zero) {
            SDL.DestroyRenderer(_rendererPtr);
            _rendererPtr = IntPtr.Zero;
        }
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

public sealed class SoftwareRenderer : IRenderer
{
    public nint SDLRenderer { get; private set; }
    public nint SDLSurface { get; private set; }
    public static ushort PX_SIZE = 1024;

    public SoftwareRenderer()
    {
        SDLSurface = SDL.CreateSurface(PX_SIZE, PX_SIZE, SDL.PixelFormat.RGBA8888);
        if (SDLSurface == IntPtr.Zero) throw new Exception("Can't create SDL surface: " + SDL.GetError());

        SDLRenderer = SDL.CreateSoftwareRenderer(SDLSurface);
        if (SDLRenderer == IntPtr.Zero) throw new Exception("Can't create SDL Software Renderer: " + SDL.GetError());

        SDL.SetDefaultTextureScaleMode(SDLRenderer, SDL.ScaleMode.Nearest);
    }

    public Texture ProduceTexture(ushort w, ushort h)
    {
        SDL.RenderPresent(SDLRenderer);
        var region = new Rectangle(0, 0, w, h);
        var tex = new Texture(TextureUtil.ExtractRegion(SDLSurface, region));
        SDL.RenderClear(SDLRenderer);
        return tex;
    }

    public void Dispose()
    {
        if (SDLRenderer != IntPtr.Zero)
        {
            SDL.DestroyRenderer(SDLRenderer);
            SDLRenderer = IntPtr.Zero;
        }

        if (SDLSurface != IntPtr.Zero)
        {
            SDL.DestroySurface(SDLSurface);
            SDLSurface = IntPtr.Zero;
        }
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