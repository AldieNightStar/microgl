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
    public Surface Surface { get; private set; }

    public SoftwareRenderer(ushort w, ushort h)
    {
        Surface = new Surface(w, h);

        SDLRenderer = SDL.CreateSoftwareRenderer(Surface);
        if (SDLRenderer == IntPtr.Zero) throw new Exception("Can't create SDL Software Renderer: " + SDL.GetError());

        SDL.SetDefaultTextureScaleMode(SDLRenderer, SDL.ScaleMode.Nearest);
    }

    public Texture ProduceTexture(ushort w, ushort h)
    {
        SDL.RenderPresent(SDLRenderer);
        var region = new Rectangle(0, 0, w, h);
        var tex = new Texture(new Surface(TextureUtil.ExtractRegion(Surface, region)));
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

        if (Surface == default)
        {
            Surface.Dispose();
            Surface = default;
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