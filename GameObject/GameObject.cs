using System.Numerics;

namespace microgl;

public abstract class GameObject : GameObjectBase, IDisposable
{
    private Transform _parentTransform = Transform.Identity;

    public Transform Transform { get; set; } = Transform.Identity;
    public Transform GlobalTransform { get; private set; } = Transform.Identity;
    public virtual bool IsAlive { get; set; } = true;

    public Vector2 ToLocal(Vector2 global) => GlobalTransform.ToLocal(global);
    public Vector2 ToGlobal(Vector2 local) => GlobalTransform.ToGlobal(local);

    public void Move(Vector2 vec) => Transform = Transform.Translate(vec);
    public void Rotate(float rad) => Transform = Transform.Rotate(rad);
    public void Resize(Vector2 scale) => Transform = Transform.Scale(scale);

    public void GlobalMove(Vector2 vec) => Transform = Transform.GlobalTranslate(vec);
    public void GlobalRotate(float rad) => Transform = Transform.GlobalRotate(rad);

    public void Delete() => IsAlive = false;

    public Vector2 Position
    {
        get => Transform.Position;
        set
        {
            Transform = Transform.WithPosition(value);
            GlobalTransform = _parentTransform * Transform;
        }
    }

    public Vector2 GlobalPosition
    {
        get => GlobalTransform.Position;
        set
        {
            var localPos = _parentTransform.ToLocal(value);
            Transform = Transform.WithPosition(localPos);
            GlobalTransform = _parentTransform * Transform;
        }
    }

    public float Rotation
    {
        get => Transform.Rotation;
        set
        {
            Transform = Transform.WithRotation(value);
            GlobalTransform = _parentTransform * Transform;
        }
    }

    public float GlobalRotation
    {
        get => GlobalTransform.Rotation;
        set
        {
            float localAngle = value - _parentTransform.Rotation;
            Transform = Transform.WithRotation(localAngle);
            GlobalTransform = _parentTransform * Transform;
        }
    }

    public Vector2 Scale
    {
        get => Transform.ScaleFactor;
        set
        {
            Transform = Transform.WithScale(value);
            GlobalTransform = _parentTransform * Transform;
        }
    }

    public Vector2 GlobalScale
    {
        get => GlobalTransform.ScaleFactor;
        set
        {
            var parentScale = _parentTransform.ScaleFactor;
            var localScale = new Vector2(
                parentScale.X != 0f ? value.X / parentScale.X : value.X,
                parentScale.Y != 0f ? value.Y / parentScale.Y : value.Y
            );
            Transform = Transform.WithScale(localScale);
            GlobalTransform = _parentTransform * Transform;
        }
    }

    // --- Public API (Called by the game or GameObjectGroup) ---

    public void Init(Drawer d) => Init(d, Transform.Identity);
    public void Update(Drawer d) => Update(d, Transform.Identity);
    public void Draw(Drawer d) => Draw(d, Transform.Identity);

    // --- Internal Hierarchical Overloads ---

    internal void Init(Drawer d, Transform parentTransform)
    {
        _parentTransform = parentTransform;
        GlobalTransform = parentTransform * Transform;
        OnInit(d);
    }

    internal void Update(Drawer d, Transform parentTransform)
    {
        if (!IsAlive) return;
        _parentTransform = parentTransform;
        GlobalTransform = parentTransform * Transform;
        d.Transform = GlobalTransform;
        OnUpdate(d, Time.DeltaTime);
    }

    internal void Draw(Drawer d, Transform parentTransform)
    {
        if (!IsAlive) return;
        _parentTransform = parentTransform;
        GlobalTransform = parentTransform * Transform;
        d.Transform = GlobalTransform;
        OnDraw(d, Time.DeltaTime);
    }
}