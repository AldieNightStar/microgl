using SDL3;

/// <summary>
/// Opaque handle to an SDL render-target texture created by
/// <see cref="IRenderer.CreateTargetTexture"/>. Dispose to free the SDL texture.
/// </summary>
public struct RenderTarget : IDisposable
{
    internal nint Handle { get; private set; }

    internal RenderTarget(nint handle) => Handle = handle;

    public void Dispose()
    {
        if (Handle == IntPtr.Zero) return;
        SDL.DestroyTexture(Handle);
        Handle = IntPtr.Zero;
    }
}