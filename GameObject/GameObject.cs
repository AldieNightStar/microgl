using System.Drawing;
using System.Numerics;

namespace microgl;

public abstract class GameObject : IDisposable
{
    private Transform _transform = Transform.Identity;
    private Transform _parentTransform = Transform.Identity;

    public virtual bool IsAlive { get; set; } = true;
    public void Delete() => IsAlive = false;
    public bool IsActive { get; set; } = true;

    protected abstract void OnInit(Drawer d);
    protected abstract void OnUpdate(Drawer d);
    protected abstract void OnDraw(Drawer d);
    public abstract void Dispose();

    public Transform Transform
    {
        get => _transform;
        set
        {
            _transform = value;
            GlobalTransform = _parentTransform * value;
        }
    }

    public Transform GlobalTransform { get; private set; } = Transform.Identity;

    public Vector2 Position
    {
        get => Transform.Position;
        set => Transform = Transform.WithPosition(value);
    }

    public float Rotation
    {
        get => Transform.Rotation;
        set => Transform = Transform.WithRotation(value);
    }

    public Vector2 Scale
    {
        get => Transform.ScaleFactor;
        set => Transform = Transform.WithScale(value);
    }

    public Vector2 GlobalPosition
    {
        get => GlobalTransform.Position;
        set => Transform = Transform.WithGlobalPosition(_parentTransform, value);
    }

    public float GlobalRotation
    {
        get => GlobalTransform.Rotation;
        set => Transform = Transform.WithGlobalRotation(_parentTransform, value);
    }

    public Vector2 GlobalScale
    {
        get => GlobalTransform.ScaleFactor;
        set => Transform = Transform.WithGlobalScale(_parentTransform, value);
    }

    public Vector2 CenterPoint { get; set; } = Vector2.Zero;

    public Color BlendColor { get; set; } = Color.White;

    public Vector2 ToLocal(Vector2 global) => GlobalTransform.ToLocal(global);
    public Vector2 ToGlobal(Vector2 local) => GlobalTransform.ToGlobal(local);

    public void Init(Drawer d, Transform? parent = null)
    {
        _setParent(parent);
        using var scope = new DrawerScope(d, _renderTransform(), GlobalTransform);
        OnInit(d);
    }

    public void Update(Drawer d, Transform? parent = null)
    {
        if (!IsAlive || !IsActive) return;
        _setParent(parent);
        using var scope = new DrawerScope(d, _renderTransform(), GlobalTransform);
        OnUpdate(d);
    }

    public void Draw(Drawer d, Transform? parent = null)
    {
        if (!IsAlive || !IsActive) return;
        _setParent(parent);
        using var scope = new DrawerScope(d, _renderTransform(), GlobalTransform, BlendColor);
        OnDraw(d);
    }

    private void _setParent(Transform? parent)
    {
        _parentTransform = parent ?? Transform.Identity;
        GlobalTransform = _parentTransform * Transform;
    }

    private Transform _renderTransform() => CenterPoint == Vector2.Zero
        ? GlobalTransform
        : GlobalTransform.Translate(-CenterPoint);
}
