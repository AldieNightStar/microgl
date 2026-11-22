using SDL3;

namespace microgl;

internal struct EventBuffer(int initialCapacity = 64)
{
    private SDL.Event[] _items = new SDL.Event[initialCapacity];
    private int _count = 0;

    public void Add(ref SDL.Event ev)
    {
        if (_count == _items.Length)
            Array.Resize(ref _items, _items.Length * 2);

        _items[_count++] = ev;
    }

    public ReadOnlySpan<SDL.Event> Flush()
    {
        var span = _items.AsSpan(0, _count);
        _count = 0;
        return span;
    }
}
