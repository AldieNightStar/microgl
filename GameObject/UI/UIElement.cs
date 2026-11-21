using System;
using System.Drawing;
using System.Numerics;

namespace microgl;

public abstract class UIElement : GameObject
{
    private const float MAX_SIZE_DEFAULT = 1000;

    private Vector2 _availableSize = new(float.MaxValue, float.MaxValue);
    private bool _centerOrigin;

    public Vector2 Size
    {
        get => field;
        set {
            field = value;
            _applySize(value);
        }
    }

    public Vector2 MinSize
    {
        get => field;
        set
        {
            field = value;
            _applySize(value);
        }
    }

    public Vector2 MaxSize
    {
        get => field;
        set
        {
            field = value;
            _applySize(value);
        }
    } = new(MAX_SIZE_DEFAULT, MAX_SIZE_DEFAULT);

    public Vector2 AvailableSize
    {
        get => _availableSize;
        set
        {
            _availableSize = value;
            _applySize(value);
        }
    }

    public bool CenterOrigin
    {
        get => _centerOrigin;
        set
        {
            if (_centerOrigin == value) return;
            _centerOrigin = value;
            _updateOrigin();
        }
    }

    public Rectangle LocalBounds => new(0, 0, (int)Size.X, (int)Size.Y);

    public Rectangle Bounds
    {
        get
        {
            Vector2 topLeft = Position - CenterPoint;
            return new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)Size.X, (int)Size.Y);
        }
        set
        {
            Size = new Vector2(value.Width, value.Height);
            Position = new Vector2(value.X, value.Y) + CenterPoint;
        }
    }

    public Rectangle GlobalBounds => QuadUtil.TransformedBounds(GlobalTransform, Size);

    protected UIElement()
    {
    }

    protected UIElement(Rectangle bounds, bool centerOrigin = false)
    {
        Position = new Vector2(bounds.X, bounds.Y);
        Size = new Vector2(bounds.Width, bounds.Height);
        _centerOrigin = centerOrigin;
        _updateOrigin();
    }

    public bool ContainsPoint(Vector2 screenPos)
    {
        Vector2 local = ToLocal(screenPos);
        return local.X >= 0f && local.X <= Size.X &&
               local.Y >= 0f && local.Y <= Size.Y;
    }

    private Vector2 _clamped(Vector2 newSize) => Vector2.Clamp(newSize, Vector2.Zero, MaxSize);

    private void _applySize(Vector2 newSize)
    {
        var clampedSize = _clamped(newSize);
        if (clampedSize == Size) return;
        Size = clampedSize;
        _updateOrigin();
        OnResized();
    }

    private void _updateOrigin()
    {
        CenterPoint = _centerOrigin ? Size / 2f : Vector2.Zero;
    }

    protected virtual void OnResized() { }
}
