using microgl;
using SDL3;

internal struct RenderTarget : IDisposable
{
    internal nint Handle { get; private set; }

    internal RenderTarget(nint handle) => Handle = handle;

    public static RenderTarget CreateTargetTexture(IRenderer renderer, int width, int height)
    {
        var handle = SDL.CreateTexture(renderer.GetNativePointer(), SDL.PixelFormat.RGBA8888, SDL.TextureAccess.Target, width, height);
        if (handle == IntPtr.Zero)
            throw new Exception($"CreateTargetTexture failed: {SDL.GetError()}");
        return new RenderTarget(handle);
    }

    public static RenderTarget? GetTarget(IRenderer renderer)
    {
        var handle = SDL.GetRenderTarget(renderer.GetNativePointer());
        return handle == IntPtr.Zero ? null : new RenderTarget(handle);
    }

    public static void SetTarget(IRenderer renderer, RenderTarget? target)
    {
        SDL.SetRenderTarget(renderer.GetNativePointer(), target?.Handle ?? IntPtr.Zero);
    }

    public void RenderToScreen(IRenderer renderer, SDL.FRect dstRect)
    {
        SDL.RenderTexture(renderer.GetNativePointer(), Handle, IntPtr.Zero, ref dstRect);
    }

    public void SetBlendMode(SDL.BlendMode mode)
    {
        SDL.SetTextureBlendMode(Handle, mode);
    }

    public void Dispose()
    {
        if (Handle == IntPtr.Zero) return;
        SDL.DestroyTexture(Handle);
        Handle = IntPtr.Zero;
    }
}