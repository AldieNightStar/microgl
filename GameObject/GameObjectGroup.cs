namespace microgl;

public class GameObjectGroup : GameObject
{
    private readonly List<GameObject> _list;
    private readonly List<GameObject> _queued = new();
    private readonly IReadOnlyList<GameObject> _readOnlyList;

    public event Action? OnChildrenChanged;

    public GameObjectGroup() : this(new List<GameObject>()) { }

    public GameObjectGroup(List<GameObject> objects)
    {
        _list = objects;
        _readOnlyList = objects.AsReadOnly();
    }

    protected override void OnInit(Drawer d)
    {
        foreach (var el in _list)
            el.Init(d, d.WorldTransform);
    }

    protected override void OnUpdate(Drawer d)
    {
        bool removed = _updateAndRemoveDead(d);
        bool added = _addQueued(d);
        if (removed || added) _emitChanged();
    }

    private bool _updateAndRemoveDead(Drawer d)
    {
        int countBefore = _list.Count;
        for (int i = _list.Count - 1; i >= 0; i--)
        {
            var gameObject = _list[i];
            if (gameObject.IsAlive) gameObject.Update(d, d.WorldTransform);
            if (gameObject.IsAlive) continue;

            gameObject.Dispose();
            _list.RemoveAt(i);
        }
        return _list.Count != countBefore;
    }

    private bool _addQueued(Drawer d)
    {
        if (_queued.Count == 0) return false;

        foreach (var el in _queued)
        {
            _list.Add(el);
            el.Init(d, d.WorldTransform);
        }
        _queued.Clear();
        return true;
    }

    protected override void OnDraw(Drawer d)
    {
        foreach (var el in _list)
            el.Draw(d, d.WorldTransform);
    }

    public override void Dispose()
    {
        OnChildrenChanged = null;
        Clear();
    }

    public void Add(GameObject obj)
    {
        _queued.Add(obj);
    }

    public IReadOnlyList<GameObject> GetChildren() => _readOnlyList;

    public IEnumerable<T> GetChildren<T>() where T : GameObject
        => _list.OfType<T>().Concat(_queued.OfType<T>());

    public void CollectChildren<T>(List<T> target) where T : GameObject
    {
        target.Clear();
        _appendOfType(_list, target);
        _appendOfType(_queued, target);
    }

    private static void _appendOfType<T>(List<GameObject> source, List<T> target) where T : GameObject
    {
        foreach (var el in source)
            if (el is T match) target.Add(match);
    }

    public void Remove(GameObject obj)
    {
        if (_list.Contains(obj))
            obj.Delete();
        else
            _removeFromQueued(obj);
    }

    private void _removeFromQueued(GameObject obj)
    {
        if (!_queued.Remove(obj)) return;
        obj.Dispose();
        _emitChanged();
    }

    public void Clear()
    {
        if (_list.Count == 0 && _queued.Count == 0) return;

        _disposeAll(_list);
        _disposeAll(_queued);
        _emitChanged();
    }

    private static void _disposeAll(List<GameObject> objects)
    {
        foreach (var el in objects)
            el.Dispose();
        objects.Clear();
    }

    private void _emitChanged() => OnChildrenChanged?.Invoke();
}
