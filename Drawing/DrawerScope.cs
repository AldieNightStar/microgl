using System.Drawing;

namespace microgl;

internal readonly ref struct DrawerScope
{
    private readonly Drawer _d;
    private readonly Transform _prevTransform;
    private readonly Transform _prevWorld;
    private readonly Color _prevBlend;
    private readonly bool _restoreBlend;

    public DrawerScope(Drawer d, Transform renderTransform, Transform worldTransform)
    {
        _d             = d;
        _prevTransform = d.Transform;
        _prevWorld     = d.WorldTransform;
        _prevBlend     = default;
        _restoreBlend  = false;
        d.Transform      = renderTransform;
        d.WorldTransform = worldTransform;
    }

    public DrawerScope(Drawer d, Transform renderTransform, Transform worldTransform, Color blendColor)
    {
        _d             = d;
        _prevTransform = d.Transform;
        _prevWorld     = d.WorldTransform;
        _prevBlend     = d.BlendColor;
        _restoreBlend  = true;
        d.Transform      = renderTransform;
        d.WorldTransform = worldTransform;
        d.BlendColor     = blendColor;
    }

    public void Dispose()
    {
        _d.Transform      = _prevTransform;
        _d.WorldTransform = _prevWorld;
        if (_restoreBlend)
            _d.BlendColor = _prevBlend;
    }
}
