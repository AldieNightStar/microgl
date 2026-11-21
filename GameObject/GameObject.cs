using System.Drawing;
using System.Numerics;

namespace microgl;

public abstract class GameObject : IDisposable
{
    private Transform _parentTransform = Transform.Identity;

    public virtual bool IsAlive { get; set; } = true;
    public Vector2 CenterPoint { get; set; } = Vector2.Zero;
    public Color BlendColor { get; set; } = Color.White;

    public Transform Transform { get; set; } = Transform.Identity;
    public Transform GlobalTransform => _parentTransform * Transform;

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

    public Vector2 ToLocal(Vector2 gpos) => GlobalTransform.ToLocal(gpos);
    public Vector2 ToGlobal(Vector2 gpos) => GlobalTransform.ToGlobal(gpos);

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

    public void Init(Drawer d, GameObject parent) => Init(d, parent.GlobalTransform);
    public void Update(Drawer d, GameObject parent) => Update(d, parent.GlobalTransform);
    public void Draw(Drawer d, GameObject parent) => Draw(d, parent.GlobalTransform);

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
