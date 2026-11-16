using System.Numerics;

namespace microgl;

public abstract class GameObject : IDisposable
{
    public Transform Transform { get; set; } = Transform.Identity;
    public Transform GlobalTransform { get; private set; } = Transform.Identity;
    public virtual bool IsAlive { get; set; } = true;

    public Vector2 ToLocal(Vector2 global) => GlobalTransform.ToLocal(global);
    public Vector2 ToGlobal(Vector2 local) => GlobalTransform.ToGlobal(local);

    public void Move(Vector2 vec) => Transform = Transform.Move(vec);
    public void Move(float x, float y) => Transform = Transform.Move(new Vector2(x, y));
    public void Rotate(float rad) => Transform = Transform.Rotate(rad);
    public void Scale(Vector2 scale) => Transform = Transform.Scale(scale);

    public void GlobalMove(Vector2 vec) => Transform = Transform.GlobalMove(vec);
    public void GlobalRotate(float rad) => Transform = Transform.GlobalRotate(rad);

    public Vector2 Position
    {
        get => Transform.Position;
        set => Transform = Transform.WithPosition(value);
    }

    public Vector2 GlobalPosition
    {
        get => GlobalTransform.Position;
        set
        {
            Transform = Transform.WithPosition(value);
            GlobalTransform = Transform;
        }
    }

    // --- Public API (Called by the game or GameObjectGroup) ---

    public void Init(Drawer d) => OnInit(d);
    public void Update(Drawer d) => Update(d, Transform.Identity);
    public void Draw(Drawer d) => Draw(d, Transform.Identity);

    // --- Internal Hierarchical Overloads ---

    internal void Update(Drawer d, Transform parentTransform)
    {
        if (!IsAlive) return;
        GlobalTransform = parentTransform * Transform;
        d.Transform = GlobalTransform;
        OnUpdate(d, Time.DeltaTime);
    }

    internal void Draw(Drawer d, Transform parentTransform)
    {
        if (!IsAlive) return;
        GlobalTransform = parentTransform * Transform;
        d.Transform = GlobalTransform;
        OnDraw(d, Time.DeltaTime);
    }

    // --- Protected Hooks (Implemented by subclasses) ---

    protected abstract void OnInit(Drawer d);
    protected abstract void OnUpdate(Drawer d, double dt);
    protected abstract void OnDraw(Drawer d, double dt);
    public abstract void Dispose();
}

public class GameObjectGroup : GameObject
{
    private readonly List<GameObject> _list;
    private readonly List<GameObject> _queued = new();

    public GameObjectGroup() : this(new List<GameObject>()) { }

    public GameObjectGroup(List<GameObject> objects)
    {
        _list = objects;
    }

    protected override void OnInit(Drawer d)
    {
        foreach (var el in _list)
        {
            el.Init(d);
        }
    }

    protected override void OnUpdate(Drawer d, double dt)
    {
        // Update each object in reverse order (to remove dead objects)
        for (int i = _list.Count - 1; i >= 0; i--)
        {
            var gameObject = _list[i];
            gameObject.Update(d, GlobalTransform);

            if (!gameObject.IsAlive)
            {
                gameObject.Dispose();
                _list.RemoveAt(i);
            }
        }

        // In progress of adding new objects
        if (_queued.Count > 0)
        {
            foreach (var el in _queued)
            {
                _list.Add(el);
                el.Init(d);
            }
            _queued.Clear();
        }
    }

    protected override void OnDraw(Drawer d, double dt)
    {
        foreach (var el in _list)
        {
            el.Draw(d, GlobalTransform);
        }
    }

    public override void Dispose()
    {
        foreach (var el in _list)
        {
            el.Dispose();
        }
        _list.Clear();

        foreach (var el in _queued)
        {
            el.Dispose();
        }
        _queued.Clear();
    }

    public void Add(GameObject obj)
    {
        _queued.Add(obj);
    }
}
