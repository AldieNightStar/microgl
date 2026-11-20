namespace microgl;

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
            el.Init(d, d.WorldTransform);
    }

    protected override void OnUpdate(Drawer d)
    {
        // Update each object in reverse order (to remove dead objects)
        for (int i = _list.Count - 1; i >= 0; i--)
        {
            var gameObject = _list[i];
            if (gameObject.IsAlive) gameObject.Update(d, d.WorldTransform);

            // Later, i need to check that object s 
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
                el.Init(d, d.WorldTransform);
            }
            _queued.Clear();
        }
    }

    protected override void OnDraw(Drawer d)
    {
        foreach (var el in _list)
            el.Draw(d, d.WorldTransform);
    }

    public override void Dispose()
    {
        foreach (var el in _list)
            el.Dispose();
        _list.Clear();

        foreach (var el in _queued)
            el.Dispose();
        _queued.Clear();
    }

    public void Add(GameObject obj)
    {
        _queued.Add(obj);
    }

    public IReadOnlyList<GameObject> GetChildren() => _list.AsReadOnly();

    public IEnumerable<T> GetChildren<T>() where T : GameObject
        => _list.OfType<T>().Concat(_queued.OfType<T>());

    public void Remove(GameObject obj)
    {
        if (_list.Contains(obj))
            obj.Delete();
        else
            _removeFromQueued(obj);
    }

    private void _removeFromQueued(GameObject obj)
    {
        if (_queued.Remove(obj))
            obj.Dispose();
    }

    public void Clear()
    {
        foreach (var el in _list)
            el.Dispose();
        _list.Clear();

        foreach (var el in _queued)
            el.Dispose();
        _queued.Clear();
    }
}