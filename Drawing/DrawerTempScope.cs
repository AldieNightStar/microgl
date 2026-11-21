using System.Drawing;

namespace microgl;

internal readonly ref struct DrawerTempScope
{
    private readonly Drawer _d;
    private readonly Transform _prevTransform;
    private readonly Transform _prevWorld;
    private readonly Color _prevBlend;

    public DrawerTempScope(Drawer d, Transform renderTransform, Transform worldTransform, Color blendColor)
    {
        _d = d;
        _prevTransform = d.Transform;
        _prevWorld = d.WorldTransform;
        _prevBlend = d.BlendColor;
        d.Transform = renderTransform;
        d.WorldTransform = worldTransform;
        d.BlendColor = blendColor;
    }

    public void Dispose()
    {
        _d.Transform = _prevTransform;
        _d.WorldTransform = _prevWorld;
        _d.BlendColor = _prevBlend;
    }
}
