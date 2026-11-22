namespace microgl;

internal sealed class ResourceCache<T, V> : IDisposable where T : class
{
    private readonly Dictionary<T, V> _entries = new();
    private readonly Func<T, V> _create;
    private readonly Action<V> _release;
    private bool _disposed;

    public ResourceCache(Func<T, V> create, Action<V> release)
    {
        _create = create;
        _release = release;
        FreeingResourceBus<T>.Freeing += _onFreeing;
    }

    public V Get(T key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_entries.TryGetValue(key, out var value))
        {
            return value;
        }

        value = _create(key);
        _entries[key] = value;
        return value;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        FreeingResourceBus<T>.Freeing -= _onFreeing;
        _releaseAll();
    }

    private void _onFreeing(T key)
    {
        if (_entries.Remove(key, out var value)) _release(value);
    }

    private void _releaseAll()
    {
        foreach (var value in _entries.Values) _release(value);
        _entries.Clear();
    }
}
