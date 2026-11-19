using System.Drawing;
using System.Numerics;

namespace microgl;

public abstract class GameObject : IDisposable
{
    public virtual bool IsAlive { get; set; } = true;

    protected abstract void OnInit(Drawer d);
    protected abstract void OnUpdate(Drawer d);
    protected abstract void OnDraw(Drawer d);
    public abstract void Dispose();

    private Transform _parentTransform = Transform.Identity;

    public Transform Transform
    {
        get => field;
        set
        {
            field = value;
            _recalculateGlobalTransform();
        }
    } = Transform.Identity;

    public Transform GlobalTransform { get; private set; } = Transform.Identity;
    public Color BlendColor { get; set; } = Color.White;

    public Vector2 ToLocal(Vector2 global) => GlobalTransform.ToLocal(global);
    public Vector2 ToGlobal(Vector2 local) => GlobalTransform.ToGlobal(local);

    public void Move(Vector2 vec) => Transform = Transform.Translate(vec);
    public void Rotate(float rad) => Transform = Transform.Rotate(rad);
    public void Resize(Vector2 scale) => Transform = Transform.Scale(scale);

    public void GlobalMove(Vector2 vec) => Transform = Transform.GlobalTranslate(vec);
    public void GlobalRotate(float rad) => Transform = Transform.GlobalRotate(rad);

    public void Delete() => IsAlive = false;

    public Vector2 CenterPoint
    {
        get => field;
        set
        {
            field = value;
            _recalculateGlobalTransform();
        }
    } = Vector2.Zero;

    public Vector2 Position
    {
        get => Transform.Position;
        set => Transform = Transform.WithPosition(value);
    }

    public Vector2 GlobalPosition
    {
        get => GlobalTransform.Position;
        set => Transform = Transform.WithPosition(_parentTransform.ToLocal(value));
    }

    public float Rotation
    {
        get => Transform.Rotation;
        set => Transform = Transform.WithRotation(value);
    }

    public float GlobalRotation
    {
        get => GlobalTransform.Rotation;
        set => Transform = Transform.WithRotation(value - _parentTransform.Rotation);
    }

    public Vector2 Scale
    {
        get => Transform.ScaleFactor;
        set => Transform = Transform.WithScale(value);
    }

    public Vector2 GlobalScale
    {
        get => GlobalTransform.ScaleFactor;
        set
        {
            var parentScale = _parentTransform.ScaleFactor;
            Transform = Transform.WithScale(new Vector2(
                parentScale.X != 0f ? value.X / parentScale.X : value.X,
                parentScale.Y != 0f ? value.Y / parentScale.Y : value.Y
            ));
        }
    }

    private void _recalculateGlobalTransform()
    {
        var baseGlobal = _parentTransform * Transform;
        GlobalTransform = CenterPoint == Vector2.Zero
            ? baseGlobal
            : baseGlobal * Transform.Identity.Translate(-CenterPoint);
    }

    // --- Public API (Called by the game or GameObjectGroup) ---

    public void Init(Drawer d) => Init(d, Transform.Identity);
    public void Update(Drawer d) => Update(d, Transform.Identity);
    public void Draw(Drawer d) => Draw(d, Transform.Identity);

    // --- Internal Hierarchical Overloads ---

    public void Init(Drawer d, Transform parentTransform)
    {
        _parentTransform = parentTransform;
        _recalculateGlobalTransform();
        OnInit(d);
    }

    public void Update(Drawer d, Transform parentTransform)
    {
        if (!IsAlive) return;
        _parentTransform = parentTransform;
        _recalculateGlobalTransform();
        d.Transform = GlobalTransform;
        OnUpdate(d);
    }

    public void Draw(Drawer d, Transform parentTransform)
    {
        if (!IsAlive) return;
        _parentTransform = parentTransform;
        _recalculateGlobalTransform();
        d.Transform = GlobalTransform;
        d.BlendColor = BlendColor;
        OnDraw(d);
    }
}