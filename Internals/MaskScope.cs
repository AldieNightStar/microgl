using System.Drawing;
using SDL3;

namespace microgl;

public ref struct MaskScope
{
    private readonly Drawer        _drawer;
    private readonly Texture       _mask;
    private readonly Rectangle     _dstRect;
    private readonly RenderTarget  _target;
    private readonly RenderTarget? _prevTarget;
    private bool                   _disposed;

    internal MaskScope(Drawer drawer, Texture mask, Rectangle dstRect)
    {
        _drawer = drawer;
        _mask = mask;
        _dstRect = dstRect;

        var screen = drawer.ScreenBounds;
        _target = RenderTarget.CreateTargetTexture(drawer.Renderer, screen.Width, screen.Height);
        _prevTarget = RenderTarget.GetTarget(drawer.Renderer);
        RenderTarget.SetTarget(drawer.Renderer, _target);

        SDL.SetRenderDrawColor(drawer.Renderer.GetNativePointer(), 0, 0, 0, 0);
        SDL.RenderClear(drawer.Renderer.GetNativePointer());
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _composeOnGpu();

        _target.Dispose();
    }

    private void _composeOnGpu()
    {
        _mask.SetBlendMode(_drawer.Renderer, BlendModes.MaskMultiplyMode);
        _drawer.DrawTexture(_mask, _dstRect);

        RenderTarget.SetTarget(_drawer.Renderer, _prevTarget);
        _target.SetBlendMode(BlendModes.PremultipliedBlit);
        _drawer.DrawScreenQuad(_target.Handle, _dstRect);
    }
}
