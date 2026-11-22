using System.Drawing;
using System.Numerics;

namespace microgl;

public abstract class GameObject : IDisposable
{
    private Transform _parentTransform = Transform.Identity;
    private Transform _localTransform = Transform.Identity;
    private bool _dirty = true;

    public virtual bool IsAlive { get; set; } = true;
    public Vector2 CenterPoint { get; set; } = Vector2.Zero;
    public Color BlendColor { get; set; } = Color.White;

    public Vector2 Position { get => field; set { field = value; _dirty = true; } }
    public float Rotation { get => field; set { field = value; _dirty = true; } }
    public Vector2 Scale { get => field; set { field = value; _dirty = true; } } = Vector2.One;
    public Vector2 Skew { get => field; set { field = value; _dirty = true; } }

    public float RotationDegrees
    {
        get => Rotation * (180f / MathF.PI);
        set => Rotation = value * (MathF.PI / 180f);
    }

    public Transform Transform
    {
        get
        {
            if (_dirty)
            {
                _localTransform = Transform.Create(Position, Rotation, Scale, Skew);
                _dirty = false;
            }
            return _localTransform;
        }
    }

    public Transform GlobalTransform => _parentTransform * Transform;

    public Vector2 GlobalPosition
    {
        get => GlobalTransform.Position;
        set => Position = _parentTransform.ToLocal(value);
    }

    public float GlobalRotation
    {
        get => _parentTransform.Rotation + Rotation;
        set => Rotation = value - _parentTransform.Rotation;
    }

    public float GlobalRotationDegrees
    {
        get => GlobalRotation * (180f / MathF.PI);
        set => GlobalRotation = value * (MathF.PI / 180f);
    }

    public Vector2 GlobalScale
    {
        get => _parentTransform.ScaleFactor * Scale;
        set
        {
            var ps = _parentTransform.ScaleFactor;
            Scale = new Vector2(
                ps.X != 0f ? value.X / ps.X : 0f,
                ps.Y != 0f ? value.Y / ps.Y : 0f);
        }
    }

    public Vector2 Right => new(MathF.Cos(GlobalRotation), MathF.Sin(GlobalRotation));
    public Vector2 Down => new(-MathF.Sin(GlobalRotation), MathF.Cos(GlobalRotation));

    public void MoveLocal(Vector2 delta)
    {
        var cos = MathF.Cos(Rotation);
        var sin = MathF.Sin(Rotation);
        Position += new Vector2(
            delta.X * cos - delta.Y * sin,
            delta.X * sin + delta.Y * cos);
    }

    public void LookAt(Vector2 worldTarget)
    {
        var dir = worldTarget - GlobalPosition;
        if (dir.LengthSquared() > 0f)
            GlobalRotation = MathF.Atan2(dir.Y, dir.X);
    }

    public Vector2 ToLocal(Vector2 worldPos) => GlobalTransform.ToLocal(worldPos);
    public Vector2 ToGlobal(Vector2 localPos) => GlobalTransform.ToGlobal(localPos);

    public void Init(Drawer d, Transform? parent = null)
    {
        using (_executeInTempScope(d, parent, BlendColor))
        {
            OnInit(d);
        }
    }

    public void Update(Drawer d, Transform? parent = null)
    {
        if (!IsAlive) return;
        using (_executeInTempScope(d, parent, BlendColor))
        {
            OnUpdate(d);
        }
    }

    public void Draw(Drawer d, Transform? parent = null)
    {
        if (!IsAlive) return;
        using (_executeInTempScope(d, parent, BlendColor))
        {
            OnDraw(d);
        }
    }

    public void Init(GameObject parent, Drawer d) => Init(d, parent.GlobalTransform);
    public void Update(GameObject parent, Drawer d) => Update(d, parent.GlobalTransform);
    public void Draw(GameObject parent, Drawer d) => Draw(d, parent.GlobalTransform);

    protected abstract void OnInit(Drawer d);
    protected abstract void OnUpdate(Drawer d);
    protected abstract void OnDraw(Drawer d);
    public abstract void Dispose();

    private DrawerTempScope _executeInTempScope(Drawer d, Transform? parent, Color blend)
    {
        _parentTransform = parent ?? Transform.Identity;
        var renderTransform = _getPivotTransform();
        return new DrawerTempScope(d, renderTransform, GlobalTransform, blend);
    }

    private Transform _getPivotTransform()
    {
        return CenterPoint == Vector2.Zero
            ? GlobalTransform
            : GlobalTransform.Translate(-CenterPoint);
    }
}
