using System.Numerics;

namespace microgl;

public abstract class GameObject : IDisposable
{
    public Transform Transform { get; set; } = Transform.Identity;
    public Transform GlobalTransform { get; private set; } = Transform.Identity;
    public virtual bool IsAlive { get; set; } = true;

    // Converts screen (0,0) coords <-> object's top-left (0,0) coords
    public Vector2 ToLocal(Vector2 global) => GlobalTransform.ToLocal(global);
    public Vector2 ToGlobal(Vector2 local) => GlobalTransform.ToGlobal(local);

    public abstract void Init(Drawer d);
    public abstract void Draw(Drawer d, double dt);
    public abstract void Update(Drawer d, double dt);
    public abstract void Dispose();

    internal void DoDraw(Drawer d) => DoDraw(d, Transform.Identity);
    internal void DoUpdate(Drawer d) => DoUpdate(d, Transform.Identity);

    internal void DoDraw(Drawer d, Transform parentTransform)
    {
        if (!IsAlive) return;
        GlobalTransform = parentTransform * Transform;
        d.Transform = GlobalTransform;
        Draw(d, Time.DeltaTime);
    }

    internal void DoUpdate(Drawer d, Transform parentTransform)
    {
        if (!IsAlive) return;
        GlobalTransform = parentTransform * Transform;
        d.Transform = GlobalTransform;
        Update(d, Time.DeltaTime);
    }
}


public class GameObjectGroup : GameObject
{
    private readonly List<GameObject> _list;
    private List<GameObject> _queued = new();

    public GameObjectGroup() : this(new List<GameObject>()) { }

    public GameObjectGroup(List<GameObject> objects)
    {
        _list = objects;
    }

    public override void Dispose()
    {
        foreach (var el in _list)
        {
            el.Dispose();
        }

        foreach (var el in _queued)
        {
            el.Dispose();
        }
    }

    public override void Draw(Drawer d, double dt)
    {
        foreach (var el in _list)
        {
            el.DoDraw(d, GlobalTransform);
        }
    }

    public override void Init(Drawer d)
    {
        foreach (var el in _list)
        {
            el.Init(d);
        }
    }

    public override void Update(Drawer d, double dt)
    {
        // Update each object in reverse order (To remove dead objects)
        for (int i = _list.Count - 1; i >= 0; i--)
        {
            var gameObject = _list[i];
            gameObject.DoUpdate(d, GlobalTransform);

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

    public void Add(GameObject obj)
    {
        _queued.Add(obj);
    }
}