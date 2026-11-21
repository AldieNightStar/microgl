using System;
using System.Drawing;
using System.Numerics;

namespace microgl;

public abstract class UIElement : GameObject
{
    private const float MAX_SIZE_DEFAULT = 1000;

    private Vector2 _requestedSize;
    private Vector2 _size;
    private Vector2 _minSize = Vector2.Zero;
    private Vector2 _maxSize = new(MAX_SIZE_DEFAULT, MAX_SIZE_DEFAULT);
    private Vector2 _availableSize = new(float.MaxValue, float.MaxValue);
    private bool _centerOrigin;

    public Vector2 Size
    {
        get => _size;
        set
        {
            _requestedSize = value;
            ApplySize();
        }
    }

    public Vector2 MinSize
    {
        get => _minSize;
        set
        {
            _minSize = value;
            ApplySize();
        }
    }

    public Vector2 MaxSize
    {
        get => _maxSize;
        set
        {
            _maxSize = value;
            ApplySize();
        }
    }

    public Vector2 AvailableSize
    {
        get => _availableSize;
        set
        {
            _availableSize = value;
            ApplySize();
        }
    }

    public Vector2 SizeLimit => Vector2.Min(_maxSize, _availableSize);

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
        _requestedSize = size;
        _size = ClampedSize();
        _centerOrigin = centerOrigin;
        _updateOrigin();
    }

    protected UIElement(Rectangle bounds, bool centerOrigin = false)
    {
        _requestedSize = new Vector2(bounds.Width, bounds.Height);
        _size = ClampedSize();
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

    private Vector2 ClampedSize() => Vector2.Clamp(_requestedSize, _minSize, SizeLimit);

    private void ApplySize()
    {
        var size = ClampedSize();
        if (_size == size) return;
        _size = size;
        _updateOrigin();
        OnResized();
    }

    private void _updateOrigin()
    {
        CenterPoint = _centerOrigin ? _size / 2f : Vector2.Zero;
    }

    protected virtual void OnResized() { }
}
