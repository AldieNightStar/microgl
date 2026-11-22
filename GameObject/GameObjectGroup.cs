namespace microgl;

public class GameObjectGroup : GameObject
{
    private readonly List<GameObject> _list;
    private readonly List<GameObject> _queued = new();
    private readonly IReadOnlyList<GameObject> _readOnlyList;
    private bool _addingLocked = false;

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
        bool foundInQueue = _processQueue(d);
        if (removed || foundInQueue) _emitChildrenChanged();
    }

    private bool _updateAndRemoveDead(Drawer d)
    {
        int countBefore = _list.Count;
        int lastIndex = _list.Count - 1;
        for (int i = lastIndex; i >= 0; i--)
        {
            if (i >= _list.Count) continue;
            var gameObject = _list[i];
            if (gameObject.IsAlive) gameObject.Update(d, d.WorldTransform);
            if (!_isStillAt(i, gameObject) || gameObject.IsAlive) continue;

            gameObject.Dispose();
            _list.RemoveAt(i);
        }
        return _list.Count != countBefore;
    }

    private bool _processQueue(Drawer d)
    {
        if (_queued.Count == 0) return false;

        _addingLocked = true;
        while (_queued.Count > 0)
        {
            var el = _queued[0];
            _queued.RemoveAt(0);
            _list.Add(el);
            el.Init(d, d.WorldTransform);
        }
        _addingLocked = false;
        return true;
    }

    private bool _isStillAt(int index, GameObject gameObject) =>
        index < _list.Count && ReferenceEquals(_list[index], gameObject);

    protected override void OnDraw(Drawer d)
    {
        for (int i = 0; i < _list.Count; i++)
            _list[i].Draw(d, d.WorldTransform);
    }

    public override void Dispose()
    {
        OnChildrenChanged = null;
        Clear();
    }

    public bool Add(GameObject obj)
    {
        if (_addingLocked) return false;
        _queued.Add(obj);
        return true;
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
        {
            if (el is T typedElement) target.Add(typedElement);
        }
    }

    public void Remove(GameObject obj)
    {
        if (_list.Contains(obj))
            obj.IsAlive = false;
        else
            _removeFromQueued(obj);
    }

    private void _removeFromQueued(GameObject obj)
    {
        if (!_queued.Remove(obj)) return;
        obj.Dispose();
        _emitChildrenChanged();
    }

    public void Clear()
    {
        if (_list.Count == 0 && _queued.Count == 0) return;

        _disposeAll(_list);
        _disposeAll(_queued);
        _emitChildrenChanged();
    }

    private static void _disposeAll(List<GameObject> objects)
    {
        foreach (var el in objects)
            el.Dispose();
        objects.Clear();
    }

    private void _emitChildrenChanged() => OnChildrenChanged?.Invoke();
}
