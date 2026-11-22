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

    public void Dispose()
    {
        if (_rendererPtr == IntPtr.Zero) return;

        FreeingResourceBus<IRenderer>.Emit(this);
        _textureCache.Dispose();
        SDL.DestroyRenderer(_rendererPtr);
        _rendererPtr = IntPtr.Zero;
    }
}
