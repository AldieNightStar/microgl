using System.Drawing;
using SDL3;

namespace microgl;

/// <summary>
/// Redirects drawing into an off-screen render target, then applies a mask by
/// multiplying the target's alpha by the mask's alpha, and blits the result back
/// to the previous render target on <see cref="Dispose"/>.
///
/// Areas where nothing was drawn stay fully transparent — the background shows
/// through exactly as if the mask scope was never there.
///
/// Usage:
/// <code>
/// using (drawer.BeginMask(circleMask, 128, 128, new Rectangle(20, 20, 128, 128)))
/// {
///     drawer.DrawTexture(playerPhoto, new Rectangle(0, 0, 128, 128));
/// }
/// </code>
/// </summary>
public ref struct MaskScope
{
    // Multiply BOTH content RGB and alpha by the mask's alpha.
    //
    // After drawing content to the render target with BlendMode.Blend, the target holds:
    //   RGB = content_alpha × src_RGB   (premultiplied by content alpha)
    //   A   = content_alpha
    //
    // We want both values premultiplied by mask_alpha before the final blit:
    //   RGB_new = mask_alpha × RGB   = mask_alpha × content_alpha × src_RGB
    //   A_new   = mask_alpha × A     = mask_alpha × content_alpha
    //
    // Formula: result = (Zero × mask) + (SrcAlpha × dst)
    //   SrcAlpha = mask.alpha, so both channels are scaled by mask_alpha.
    private static readonly SDL.BlendMode MaskMultiplyMode = SDL.ComposeCustomBlendMode(
        SDL.BlendFactor.Zero, SDL.BlendFactor.SrcAlpha, SDL.BlendOperation.Add,  // RGB: mask_alpha × dst_RGB
        SDL.BlendFactor.Zero, SDL.BlendFactor.SrcAlpha, SDL.BlendOperation.Add   // Alpha: mask_alpha × dst_A
    );

    // Premultiplied alpha blit — correct since the target is now fully premultiplied.
    // Pixels with alpha=0 contribute zero RGB regardless of their stored color.
    private static readonly SDL.BlendMode PremultipliedBlit = SDL.ComposeCustomBlendMode(
        SDL.BlendFactor.One, SDL.BlendFactor.OneMinusSrcAlpha, SDL.BlendOperation.Add,
        SDL.BlendFactor.One, SDL.BlendFactor.OneMinusSrcAlpha, SDL.BlendOperation.Add
    );

    private readonly IRenderer     _renderer;
    private readonly Texture       _mask;
    private readonly RenderTarget  _target;
    private readonly RenderTarget? _prevTarget;
    private readonly SDL.FRect     _dstRect;
    private bool                   _disposed;

    internal MaskScope(Drawer drawer, Texture mask, int width, int height, Rectangle dstRect)
    {
        _renderer = drawer.Renderer;
        _mask     = mask;
        _dstRect  = new SDL.FRect { X = dstRect.X, Y = dstRect.Y, W = dstRect.Width, H = dstRect.Height };

        _target     = _renderer.CreateTargetTexture(width, height);
        _prevTarget = _renderer.GetRenderTarget();
        _renderer.SetRenderTarget(_target);

        // Clear to transparent — content draws normally on top of this
        SDL.SetRenderDrawColor(_renderer.GetNativePointer(), 0, 0, 0, 0);
        SDL.RenderClear(_renderer.GetNativePointer());

        _disposed = false;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Apply mask: scale both RGB and alpha by mask_alpha.
        // After this the target is fully premultiplied — ready for PremultipliedBlit.
        SDL.SetTextureBlendMode(_renderer.GetNativeTexture(_mask), MaskMultiplyMode);
        SDL.RenderTexture(_renderer.GetNativePointer(), _renderer.GetNativeTexture(_mask), IntPtr.Zero, IntPtr.Zero);

        _renderer.SetRenderTarget(_prevTarget);

        var dst = _dstRect;
        _renderer.SetRenderTargetBlendMode(_target, PremultipliedBlit);
        _renderer.RenderTargetToScreen(_target, dst);

        _target.Dispose();
    }
}
