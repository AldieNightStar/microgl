using System.Drawing;
using System.Numerics;

namespace microgl;

public abstract class GameObject : IDisposable
{
    // === Lifecycle ==========================================

    public virtual bool IsAlive { get; set; } = true;
    public void Delete() => IsAlive = false;

    protected abstract void OnInit(Drawer d);
    protected abstract void OnUpdate(Drawer d);
    protected abstract void OnDraw(Drawer d);
    public abstract void Dispose();

    // == Local transform ======================================================

    public Transform Transform
    {
        get => field;
        set { field = value; _recalculate(); }
    } = Transform.Identity;

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

    // == Global transform =====================================================

    public Transform GlobalTransform { get; private set; } = Transform.Identity;

    public Vector2 GlobalPosition
    {
        get => GlobalTransform.Position;
        set => Transform = Transform.WithPosition(_parent.ToLocal(value));
    }

    public float GlobalRotation
    {
        get => GlobalTransform.Rotation;
        set => Transform = Transform.WithRotation(value - _parent.Rotation);
    }

    public Vector2 GlobalScale
    {
        get => GlobalTransform.ScaleFactor;
        set
        {
            var ps = _parent.ScaleFactor;
            Transform = Transform.WithScale(new Vector2(
                ps.X != 0f ? value.X / ps.X : value.X,
                ps.Y != 0f ? value.Y / ps.Y : value.Y));
        }
    }

    // == Pivot & blend ========================================================

    /// <summary>Pivot point in local space. Shifts the draw origin without affecting GlobalPosition.</summary>
    public Vector2 CenterPoint
    {
        get => field;
        set { field = value; _recalculate(); }
    } = Vector2.Zero;

    public Color BlendColor { get; set; } = Color.White;

    // == Convenience mutators =================================================

    public void Move(Vector2 delta)       => Transform = Transform.Translate(delta);
    public void Rotate(float rad)         => Transform = Transform.Rotate(rad);
    public void Resize(Vector2 scale)     => Transform = Transform.Scale(scale);
    public void GlobalMove(Vector2 delta) => Transform = Transform.GlobalTranslate(delta);
    public void GlobalRotate(float rad)   => Transform = Transform.GlobalRotate(rad);

    // == Coordinate conversion ================================================

    public Vector2 ToLocal(Vector2 global) => GlobalTransform.ToLocal(global);
    public Vector2 ToGlobal(Vector2 local) => GlobalTransform.ToGlobal(local);

    // == Internals ============================================================

    private Transform _parent = Transform.Identity;

    private void _recalculate() => GlobalTransform = _parent * Transform;

    /// <summary>Render transform: GlobalTransform with CenterPoint pivot applied.</summary>
    private Transform _renderTransform => CenterPoint == Vector2.Zero
        ? GlobalTransform
        : GlobalTransform.Translate(-CenterPoint);

    // == Hierarchical API (called by Game, GameObjectGroup, or Drawer) =========

    public void Init(Drawer d, Transform? parent = null)
    {
        _parent = parent ?? Transform.Identity;
        _recalculate();
        OnInit(d);
    }

    public void Update(Drawer d, Transform? parent = null)
    {
        if (!IsAlive) return;
        _parent = parent ?? Transform.Identity;
        _recalculate();

        // Save and restore previous state
        using var scope = new DrawerScope(d, _renderTransform, GlobalTransform);
        OnUpdate(d);
    }

    public void Draw(Drawer d, Transform? parent = null)
    {
        if (!IsAlive) return;
        _parent = parent ?? Transform.Identity;
        _recalculate();

        // Save and restore previous state
        using var scope = new DrawerScope(d, _renderTransform, GlobalTransform, BlendColor);
        OnDraw(d);
    }
}
