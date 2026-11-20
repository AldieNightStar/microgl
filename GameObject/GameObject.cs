using System.Drawing;
using System.Numerics;

namespace microgl;

public abstract class GameObject : IDisposable
{
    private Transform _parentTransform = Transform.Identity;

    // === Lifecycle ===========================================================

    public virtual bool IsAlive { get; set; } = true;
    public void Delete() => IsAlive = false;

    protected abstract void OnInit(Drawer d);
    protected abstract void OnUpdate(Drawer d);
    protected abstract void OnDraw(Drawer d);
    public abstract void Dispose();

    // == Local transform ======================================================

    public Transform Transform { get; set; } = Transform.Identity;

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

    /// <summary>Combined parent * local transform in world/screen space.</summary>
    public Transform GlobalTransform => _parentTransform * Transform;

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

    // == Pivot & blend ========================================================

    /// <summary>Pivot point in local space. Shifts the draw origin without affecting position.</summary>
    public Vector2 CenterPoint { get; set; } = Vector2.Zero;

    public Color BlendColor { get; set; } = Color.White;

    // == Coordinate conversion ================================================

    public Vector2 ToLocal(Vector2 global) => GlobalTransform.ToLocal(global);
    public Vector2 ToGlobal(Vector2 local) => GlobalTransform.ToGlobal(local);

    // == Internals ============================================================

    private Transform _buildGlobal(Transform? parent)
    {
        _parentTransform = parent ?? Transform.Identity;
        return _parentTransform * Transform;
    }

    private Transform _renderTransform(Transform global) => CenterPoint == Vector2.Zero
        ? global
        : global.Translate(-CenterPoint);

    // == Hierarchical API (called by Game, GameObjectGroup, or Drawer) =========

    public void Init(Drawer d, Transform? parent = null)
    {
        var global = _buildGlobal(parent);
        using var scope = new DrawerScope(d, _renderTransform(global), global);
        OnInit(d);
    }

    public void Update(Drawer d, Transform? parent = null)
    {
        if (!IsAlive) return;
        var global = _buildGlobal(parent);
        using var scope = new DrawerScope(d, _renderTransform(global), global);
        OnUpdate(d);
    }

    public void Draw(Drawer d, Transform? parent = null)
    {
        if (!IsAlive) return;
        var global = _buildGlobal(parent);
        using var scope = new DrawerScope(d, _renderTransform(global), global, BlendColor);
        OnDraw(d);
    }
}
