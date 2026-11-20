using System.Diagnostics;
using System.Drawing;
using SDL3;

namespace microgl;

public interface IRenderer : IDisposable
{
    nint GetNativePointer();
    void RenderPresent();
    bool IsValid();
    nint GetNativeTexture(Texture texture);
    void SetShader(Shader? shader);
}

public sealed class GPURenderer : IRenderer
{
    private nint _rendererPtr;
    private TextureCache _textureCache;

    public GPURenderer(nint renderer)
    {
        _rendererPtr = renderer;
        if (_rendererPtr == IntPtr.Zero) throw new ArgumentException("GPU Renderer is not initialized well");

        _textureCache = new TextureCache(renderer);
    }

    public void Dispose()
    {
        if (_textureCache != null)
        {
            _textureCache.Dispose();
            _textureCache = null;
        }
        if (_rendererPtr != IntPtr.Zero)
        {
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

    public nint GetNativeTexture(Texture texture)
    {
        return _textureCache.GetCachedSDLTexture(texture);
    }

    public void SetShader(Shader? shader)
    {
        nint statePtr = shader?.StateHandle ?? IntPtr.Zero;
        if (!SDL.SetGPURenderState(_rendererPtr, statePtr))
        {
            throw new Exception($"Can't set GPU render state for shader: {SDL.GetError()}");
        }
    }
}

public sealed class SoftwareRenderer : IRenderer
{
    public nint SDLRenderer { get; private set; }
    public Surface Surface { get; private set; }
    private TextureCache _textureCache;

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
        if (_textureCache != null)
        {
            _textureCache.Dispose();
            _textureCache = null;
        }
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

    public nint GetNativeTexture(Texture texture)
    {
        return _textureCache.GetCachedSDLTexture(texture);
    }

    public void SetShader(Shader? shader)
    {
        throw new NotSupportedException("Shaders are not supported on SoftwareRenderer.");
    }
}