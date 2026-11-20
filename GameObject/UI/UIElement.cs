using System;
using System.Drawing;
using System.Numerics;

namespace microgl;

public abstract class UIElement : GameObject
{
    private const float MAX_SIZE_DEFAULT = 1000;

    private Vector2 _size;
    private Vector2 _minSize = Vector2.Zero;
    private Vector2 _maxSize = new(MAX_SIZE_DEFAULT, MAX_SIZE_DEFAULT);
    private bool _centerOrigin;

    public Vector2 Size
    {
        get => _size;
        set
        {
            var clamped = Vector2.Clamp(value, _minSize, _maxSize);
            if (_size == clamped) return;
            _size = clamped;
            _updateOrigin();
            OnResized();
        }
    }

    public Vector2 MinSize
    {
        get => _minSize;
        set
        {
            if (_minSize == value) return;
            _minSize = value;
            Size = _size;
        }
    }

    public Vector2 MaxSize
    {
        get => _maxSize;
        set
        {
            if (_maxSize == value) return;
            _maxSize = value;
            Size = _size;
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

    protected UIElement(Vector2 position, Vector2 size, bool centerOrigin = false)
    {
        Position = position;
        _size = Vector2.Clamp(size, _minSize, _maxSize);
        _centerOrigin = centerOrigin;
        _updateOrigin();
    }

    protected UIElement(Rectangle bounds, bool centerOrigin = false)
    {
        _size = Vector2.Clamp(new Vector2(bounds.Width, bounds.Height), _minSize, _maxSize);
        _centerOrigin = centerOrigin;
        _updateOrigin();
        Position = new Vector2(bounds.X, bounds.Y) + CenterPoint;
    }

    public bool ContainsPoint(Vector2 screenPos)
    {
        Vector2 local = ToLocal(screenPos);
        return local.X >= 0f && local.X <= Size.X &&
               local.Y >= 0f && local.Y <= Size.Y;
    }

    private void _updateOrigin()
    {
        CenterPoint = _centerOrigin ? _size / 2f : Vector2.Zero;
    }

    protected virtual void OnResized() { }
}
