using System.Drawing;
using System.Numerics;
using SDL3;

namespace microgl;

public ref struct MaskScope
{
    private const int QuadVertexCount = 4;

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

        _mask.SetBlendMode(_drawer.Renderer, BlendModes.MaskMultiplyMode);
        _drawer.DrawTexture(_mask, _dstRect);

        RenderTarget.SetTarget(_drawer.Renderer, _prevTarget);
        _target.SetBlendMode(BlendModes.PremultipliedBlit);
        _drawComposite();

        _target.Dispose();
    }

    private void _drawComposite()
    {
        var verts = _buildCompositeQuad();
        if (!SDL.RenderGeometry(_drawer.Renderer.GetNativePointer(), _target.Handle, verts, QuadVertexCount, QuadUtil.QUAD_INDICES, QuadUtil.QUAD_INDICES.Length))
            throw new Exception($"RenderGeometry failed: {SDL.GetError()}");
    }

    private SDL.Vertex[] _buildCompositeQuad()
    {
        var size = _drawer.ScreenSize;
        var color = ColorUtil.ToSDLColor(Color.White);
        var corners = _localCorners();
        var verts = new SDL.Vertex[QuadVertexCount];

        for (int i = 0; i < QuadVertexCount; i++)
        {
            var g = _drawer.Transform.ToGlobal(corners[i]);
            verts[i] = new SDL.Vertex
            {
                Position = new SDL.FPoint { X = g.X, Y = g.Y },
                Color = color,
                TexCoord = new SDL.FPoint { X = g.X / size.X, Y = g.Y / size.Y },
            };
        }
        return verts;
    }

    private Vector2[] _localCorners() =>
    [
        new(_dstRect.Left,  _dstRect.Top),
        new(_dstRect.Right, _dstRect.Top),
        new(_dstRect.Right, _dstRect.Bottom),
        new(_dstRect.Left,  _dstRect.Bottom),
    ];
}
