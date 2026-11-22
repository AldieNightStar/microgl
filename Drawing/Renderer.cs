using System.Drawing;
using SDL3;

namespace microgl;

public interface IRenderer : IDisposable
{
    nint GetNativePointer();
    void RenderPresent();
    nint GetNativeTexture(Texture texture);
    void SetShader(Shader? shader);
}

public sealed class GPURenderer : IRenderer
{
    private nint _rendererPtr;
    private readonly ResourceCache<Texture, nint> _textureCache;

    public GPURenderer(nint renderer)
    {
        _rendererPtr = renderer;
        if (_rendererPtr == IntPtr.Zero) throw new ArgumentException("GPU Renderer is not initialized well");

        _textureCache = new ResourceCache<Texture, nint>(
            texture => texture.ToSdlTexture(renderer),
            SDL.DestroyTexture
        );
    }

    public void Dispose()
    {
        if (_rendererPtr == IntPtr.Zero) return;

        FreeingResourceBus<IRenderer>.Emit(this);
        _textureCache.Dispose();
        SDL.DestroyRenderer(_rendererPtr);
        _rendererPtr = IntPtr.Zero;
    }

    public nint GetNativePointer() => _rendererPtr;

    public void RenderPresent() => SDL.RenderPresent(_rendererPtr);

    public bool IsValid() => _rendererPtr != IntPtr.Zero;

    public nint GetNativeTexture(Texture texture) => _textureCache.Get(texture);

    public void SetShader(Shader? shader)
    {
        nint statePtr = shader?.StateHandle ?? IntPtr.Zero;
        if (!SDL.SetGPURenderState(_rendererPtr, statePtr))
            throw new Exception($"Can't set GPU render state for shader: {SDL.GetError()}");
    }
}

public sealed class SoftwareRenderer : IRenderer
{
    public nint SDLRenderer { get; private set; }
    public Surface Surface { get; private set; }
    private readonly ResourceCache<Texture, nint> _textures;

    public SoftwareRenderer(ushort w, ushort h)
    {
        Surface = new Surface(w, h);

        SDLRenderer = SDL.CreateSoftwareRenderer(Surface);
        if (SDLRenderer == IntPtr.Zero)
        {
            Surface.Dispose();
            throw new Exception("Can't create SDL Software Renderer: " + SDL.GetError());
        }

        SDL.SetDefaultTextureScaleMode(SDLRenderer, SDL.ScaleMode.Nearest);

        nint renderer = SDLRenderer;
        _textures = new ResourceCache<Texture, nint>(texture => texture.ToSdlTexture(renderer), SDL.DestroyTexture);
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
        if (SDLRenderer == IntPtr.Zero) return;

        FreeingResourceBus<IRenderer>.Emit(this);
        _textures.Dispose();
        SDL.DestroyRenderer(SDLRenderer);
        SDLRenderer = IntPtr.Zero;
        Surface.Dispose();
    }

    public nint GetNativePointer() => SDLRenderer;

    public void RenderPresent() => SDL.RenderPresent(SDLRenderer);

    public bool IsValid() => SDLRenderer != IntPtr.Zero;

    public nint GetNativeTexture(Texture texture) => _textures.Get(texture);

    public void SetShader(Shader? shader) =>
        throw new NotSupportedException("Shaders are not supported on SoftwareRenderer.");
}
