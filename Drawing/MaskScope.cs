using System.Drawing;
using SDL3;

namespace microgl;

public ref struct MaskScope
{
    private static readonly SDL.BlendMode MaskMultiplyMode = SDL.ComposeCustomBlendMode(
        SDL.BlendFactor.Zero, SDL.BlendFactor.SrcAlpha, SDL.BlendOperation.Add,  // RGB: mask_alpha × dst_RGB
        SDL.BlendFactor.Zero, SDL.BlendFactor.SrcAlpha, SDL.BlendOperation.Add   // Alpha: mask_alpha × dst_A
    );

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

        SDL.SetTextureBlendMode(_renderer.GetNativeTexture(_mask), MaskMultiplyMode);
        SDL.RenderTexture(_renderer.GetNativePointer(), _renderer.GetNativeTexture(_mask), IntPtr.Zero, IntPtr.Zero);

        _renderer.SetRenderTarget(_prevTarget);

        var dst = _dstRect;
        _renderer.SetRenderTargetBlendMode(_target, PremultipliedBlit);
        _renderer.RenderTargetToScreen(_target, dst);

        _target.Dispose();
    }
}
