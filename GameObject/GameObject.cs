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

    // == Global transform (requires Drawer — only valid inside Update/Draw) ====

    public Vector2 GetGlobalPosition(Drawer d) => d.WorldTransform.Position;
    public void    SetGlobalPosition(Drawer d, Vector2 value) =>
        Transform = Transform.WithPosition(d.WorldTransform.ToLocal(value));

    public float GetGlobalRotation(Drawer d) => d.WorldTransform.Rotation;
    public void  SetGlobalRotation(Drawer d, float value) =>
        Transform = Transform.WithRotation(value - d.WorldTransform.Rotation);

    public Vector2 GetGlobalScale(Drawer d) => d.WorldTransform.ScaleFactor;
    public void    SetGlobalScale(Drawer d, Vector2 value)
    {
        var ps = d.WorldTransform.ScaleFactor;
        Transform = Transform.WithScale(new Vector2(
            ps.X != 0f ? value.X / ps.X : value.X,
            ps.Y != 0f ? value.Y / ps.Y : value.Y));
    }

    // == Pivot & blend ========================================================

    /// <summary>Pivot point in local space. Shifts the draw origin without affecting position.</summary>
    public Vector2 CenterPoint { get; set; } = Vector2.Zero;

    public Color BlendColor { get; set; } = Color.White;

    // == Coordinate conversion (requires Drawer — only valid inside Update/Draw) =

    public Vector2 ToLocal(Drawer d, Vector2 global) => d.WorldTransform.ToLocal(global);
    public Vector2 ToGlobal(Drawer d, Vector2 local) => d.WorldTransform.ToGlobal(local);

    // == Internals ============================================================

    private static Transform _buildGlobal(Transform? parent, Transform local) => (parent ?? Transform.Identity) * local;

    private Transform _renderTransform(Transform global) => CenterPoint == Vector2.Zero
        ? global
        : global.Translate(-CenterPoint);

    // == Hierarchical API (called by Game, GameObjectGroup, or Drawer) =========

    public void Init(Drawer d, Transform? parent = null)
    {
        var global = _buildGlobal(parent, Transform);
        using var scope = new DrawerScope(d, _renderTransform(global), global);
        OnInit(d);
    }

    public void Update(Drawer d, Transform? parent = null)
    {
        if (!IsAlive) return;
        var global = _buildGlobal(parent, Transform);
        using var scope = new DrawerScope(d, _renderTransform(global), global);
        OnUpdate(d);
    }

    public void Draw(Drawer d, Transform? parent = null)
    {
        if (!IsAlive) return;
        var global = _buildGlobal(parent, Transform);
        using var scope = new DrawerScope(d, _renderTransform(global), global, BlendColor);
        OnDraw(d);
    }
}