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
        get => Transform.RadiansToDegrees(Rotation);
        set => Rotation = Transform.DegreesToRadians(value);
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
        get => GlobalTransform.Rotation;
        set => Rotation = _parentTransform.LocalRotationFor(value);
    }

    public float GlobalRotationDegrees
    {
        get => Transform.RadiansToDegrees(GlobalRotation);
        set => GlobalRotation = Transform.DegreesToRadians(value);
    }

    public Vector2 GlobalScale
    {
        get => _parentTransform.ScaleChild(Scale);
        set => Scale = _parentTransform.LocalScaleFor(value);
    }

    public Vector2 Right => GlobalTransform.Right;
    public Vector2 Down => GlobalTransform.Down;

    public void MoveLocal(Vector2 delta) => Position += Transform.RotateVector(delta);

    public void LookAt(Vector2 worldTarget)
    {
        if (GlobalTransform.TryGetAngleTo(worldTarget, out float angle))
            GlobalRotation = angle;
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
        var pivotTransform = _getPivotTransform();
        return new DrawerTempScope(d, pivotTransform, pivotTransform, blend);
    }

    private Transform _getPivotTransform()
    {
        return CenterPoint == Vector2.Zero
            ? GlobalTransform
            : GlobalTransform.Translate(-CenterPoint);
    }
}
