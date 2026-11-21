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

    /// <summary>Creates an RGBA render-target texture of the given size.</summary>
    RenderTarget CreateTargetTexture(int width, int height);

    /// <summary>Returns the currently active render target (null = default back-buffer).</summary>
    RenderTarget? GetRenderTarget();

    /// <summary>Switches the active render target. Pass null to restore the default back-buffer.</summary>
    void SetRenderTarget(RenderTarget? target);

    /// <summary>Sets the blend mode on a render target's texture.</summary>
    void SetRenderTargetBlendMode(RenderTarget target, SDL.BlendMode mode);

    /// <summary>Draws the render target to the given screen-space rect.</summary>
    void RenderTargetToScreen(RenderTarget target, SDL.FRect dstRect);
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

    public nint GetNativePointer() => _rendererPtr;

    public void RenderPresent() => SDL.RenderPresent(_rendererPtr);

    public bool IsValid() => _rendererPtr != IntPtr.Zero;

    public nint GetNativeTexture(Texture texture) => _textureCache.GetCachedSDLTexture(texture);

    public void SetShader(Shader? shader)
    {
        nint statePtr = shader?.StateHandle ?? IntPtr.Zero;
        if (!SDL.SetGPURenderState(_rendererPtr, statePtr))
            throw new Exception($"Can't set GPU render state for shader: {SDL.GetError()}");
    }

    public RenderTarget CreateTargetTexture(int width, int height)
    {
        var handle = SDL.CreateTexture(_rendererPtr, SDL.PixelFormat.RGBA8888, SDL.TextureAccess.Target, width, height);
        if (handle == IntPtr.Zero)
            throw new Exception($"CreateTargetTexture failed: {SDL.GetError()}");
        return new RenderTarget(handle);
    }

    public RenderTarget? GetRenderTarget()
    {
        var handle = SDL.GetRenderTarget(_rendererPtr);
        return handle == IntPtr.Zero ? null : new RenderTarget(handle);
    }

    public void SetRenderTarget(RenderTarget? target) =>
        SDL.SetRenderTarget(_rendererPtr, target?.Handle ?? IntPtr.Zero);

    public void SetRenderTargetBlendMode(RenderTarget target, SDL.BlendMode mode) =>
        SDL.SetTextureBlendMode(target.Handle, mode);

    public void RenderTargetToScreen(RenderTarget target, SDL.FRect dstRect) =>
        SDL.RenderTexture(_rendererPtr, target.Handle, IntPtr.Zero, ref dstRect);
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
        _textureCache = new(SDLRenderer);
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
        if (Surface != default)
        {
            Surface.Dispose();
            Surface = default;
        }
    }

    public nint GetNativePointer() => SDLRenderer;

    public void RenderPresent() => SDL.RenderPresent(SDLRenderer);

    public bool IsValid() => SDLRenderer != IntPtr.Zero;

    public nint GetNativeTexture(Texture texture) => _textureCache.GetCachedSDLTexture(texture);

    public void SetShader(Shader? shader) =>
        throw new NotSupportedException("Shaders are not supported on SoftwareRenderer.");

    public RenderTarget CreateTargetTexture(int width, int height)
    {
        var handle = SDL.CreateTexture(SDLRenderer, SDL.PixelFormat.RGBA8888, SDL.TextureAccess.Target, width, height);
        if (handle == IntPtr.Zero)
            throw new Exception($"CreateTargetTexture failed: {SDL.GetError()}");
        return new RenderTarget(handle);
    }

    public RenderTarget? GetRenderTarget()
    {
        var handle = SDL.GetRenderTarget(SDLRenderer);
        return handle == IntPtr.Zero ? null : new RenderTarget(handle);
    }

    public void SetRenderTarget(RenderTarget? target) =>
        SDL.SetRenderTarget(SDLRenderer, target?.Handle ?? IntPtr.Zero);

    public void SetRenderTargetBlendMode(RenderTarget target, SDL.BlendMode mode) =>
        SDL.SetTextureBlendMode(target.Handle, mode);

    public void RenderTargetToScreen(RenderTarget target, SDL.FRect dstRect) =>
        SDL.RenderTexture(SDLRenderer, target.Handle, IntPtr.Zero, ref dstRect);
}
