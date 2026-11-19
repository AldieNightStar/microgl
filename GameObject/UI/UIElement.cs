using System;
using System.Drawing;
using System.Numerics;

namespace microgl;

public abstract class UIElement : GameObject
{
    private Vector2 _size;
    private bool _centerOrigin;

    /// <summary>
    /// The width and height of this UI element in local units.
    /// Automatically updates CenterPoint if CenterOrigin is enabled.
    /// </summary>
    public Vector2 Size
    {
        get => _size;
        set
        {
            if (_size == value) return;
            _size = value;
            _updateOrigin();
            OnResized();
        }
    }

    public float Width
    {
        get => _size.X;
        set => Size = new Vector2(value, _size.Y);
    }

    public float Height
    {
        get => _size.Y;
        set => Size = new Vector2(_size.X, value);
    }

    /// <summary>
    /// When true, keeps CenterPoint automatically synced to Size / 2.
    /// </summary>
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

    /// <summary>
    /// Local bounding rectangle starting at (0, 0) with Size dimensions.
    /// </summary>
    public Rectangle LocalBounds => new(0, 0, (int)Size.X, (int)Size.Y);

    /// <summary>
    /// Bounding rectangle in parent/screen coordinates accounting for Position and CenterPoint.
    /// </summary>
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

    /// <summary>
    /// Global axis-aligned bounding box transformed into world/screen space.
    /// </summary>
    public Rectangle GlobalBounds
    {
        get
        {
            Vector2 p0 = ToGlobal(Vector2.Zero);
            Vector2 p1 = ToGlobal(new Vector2(Size.X, 0f));
            Vector2 p2 = ToGlobal(Size);
            Vector2 p3 = ToGlobal(new Vector2(0f, Size.Y));

            float minX = MathF.Min(MathF.Min(p0.X, p1.X), MathF.Min(p2.X, p3.X));
            float minY = MathF.Min(MathF.Min(p0.Y, p1.Y), MathF.Min(p2.Y, p3.Y));
            float maxX = MathF.Max(MathF.Max(p0.X, p1.X), MathF.Max(p2.X, p3.X));
            float maxY = MathF.Max(MathF.Max(p0.Y, p1.Y), MathF.Max(p2.Y, p3.Y));

            return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
        }
    }

    protected UIElement()
    {
    }

    protected UIElement(Vector2 position, Vector2 size, bool centerOrigin = false)
    {
        Position = position;
        _size = size;
        _centerOrigin = centerOrigin;
        _updateOrigin();
    }

    protected UIElement(Rectangle bounds, bool centerOrigin = false)
    {
        _size = new Vector2(bounds.Width, bounds.Height);
        _centerOrigin = centerOrigin;
        _updateOrigin();
        Position = new Vector2(bounds.X, bounds.Y) + CenterPoint;
    }

    /// <summary>
    /// Checks whether a global/screen point falls inside this element's transformed bounds.
    /// </summary>
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

    /// <summary>
    /// Called whenever Size changes so subclasses can update internal layouts or textures.
    /// </summary>
    protected virtual void OnResized() { }
}
