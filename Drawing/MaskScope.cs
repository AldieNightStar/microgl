using System.Drawing;
using SDL3;

namespace microgl;

/// <summary>
/// Redirects drawing into an off-screen render target, composites it with a mask
/// using the "draw inside invisible mask" blend, then blits the result back to the
/// previous render target — all automatically on <see cref="Dispose"/>.
///
/// While the scope is active, <see cref="Drawer.DrawTexture"/> automatically uses
/// the inside-mask blend — no separate method needed.
///
/// Usage:
/// <code>
/// using (drawer.BeginMask(circleMask, 128, 128, new Rectangle(20, 20, 128, 128)))
/// {
///     drawer.DrawTexture(playerPhoto, new Rectangle(0, 0, 128, 128));
/// }
/// </code>
/// </summary>
public ref struct MaskScope : IDisposable
{
    // Writes only the mask's alpha into the target — RGB is discarded so the
    // mask texture's own color never bleeds through.
    private static readonly SDL.BlendMode MaskWriteMode = SDL.ComposeCustomBlendMode(
        SDL.BlendFactor.Zero,     SDL.BlendFactor.Zero, SDL.BlendOperation.Add,
        SDL.BlendFactor.SrcAlpha, SDL.BlendFactor.Zero, SDL.BlendOperation.Add
    );

    // Premultiplied alpha blit — prevents black/white fringing from zero-alpha pixels
    private static readonly SDL.BlendMode PremultipliedBlit = SDL.ComposeCustomBlendMode(
        SDL.BlendFactor.One, SDL.BlendFactor.OneMinusSrcAlpha, SDL.BlendOperation.Add,
        SDL.BlendFactor.One, SDL.BlendFactor.OneMinusSrcAlpha, SDL.BlendOperation.Add
    );

    private readonly Drawer        _drawer;
    private readonly IRenderer     _renderer;
    private readonly Texture       _mask;
    private readonly RenderTarget  _target;
    private readonly RenderTarget? _prevTarget;
    private readonly SDL.FRect     _dstRect;
    private bool                   _disposed;

    internal MaskScope(Drawer drawer, Texture mask, int width, int height, Rectangle dstRect)
    {
        _drawer   = drawer;
        _renderer = drawer.Renderer;
        _mask     = mask;
        _dstRect  = new SDL.FRect { X = dstRect.X, Y = dstRect.Y, W = dstRect.Width, H = dstRect.Height };

        _target     = _renderer.CreateTargetTexture(width, height);
        _prevTarget = _renderer.GetRenderTarget();
        _renderer.SetRenderTarget(_target);

        SDL.SetRenderDrawColor(_renderer.GetNativePointer(), 0, 0, 0, 0);
        SDL.RenderClear(_renderer.GetNativePointer());

        SDL.SetTextureBlendMode(_renderer.GetNativeTexture(_mask), MaskWriteMode);
        SDL.RenderTexture(_renderer.GetNativePointer(), _renderer.GetNativeTexture(_mask), IntPtr.Zero, IntPtr.Zero);

        _drawer.InMaskScope = true;
        _disposed = false;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _drawer.InMaskScope = false;

        _renderer.SetRenderTarget(_prevTarget);

        var dst = _dstRect;
        _renderer.SetRenderTargetBlendMode(_target, PremultipliedBlit);
        _renderer.RenderTargetToScreen(_target, dst);

        _target.Dispose();
    }
}
