using System.Drawing;
using System.Numerics;

namespace microgl;

public class PagedList : UITable
{
    private const int DEFAULT_PAGE_SIZE = 5;

    private readonly List<UIElement> _items = new();

    public int PageSize
    {
        get => field;
        set => field = Math.Max(1, value);
    } = DEFAULT_PAGE_SIZE;

    public int Page
    {
        get => Math.Clamp(field, 0, PageCount - 1);
        set => field = Math.Clamp(value, 0, PageCount - 1);
    }

    public int ItemCount => _collectItems().Count;
    public int PageCount => Math.Max(1, (ItemCount + PageSize - 1) / PageSize);
    public bool HasNextPage => Page < PageCount - 1;
    public bool HasPreviousPage => Page > 0;

    public PagedList(Rectangle bounds, int pageSize = DEFAULT_PAGE_SIZE, GameObjectGroup? group = null, bool centerOrigin = false)
        : base(bounds, group, centerOrigin)
    {
        PageSize = pageSize;
    }

    public void NextPage() => Page++;
    public void PreviousPage() => Page--;

    private List<UIElement> _collectItems()
    {
        Group.CollectChildren(_items);
        _items.RemoveAll(static item => !item.IsAlive);
        return _items;
    }

    private void _showPage()
    {
        int first = Page * PageSize;
        int index = 0;

        foreach (var item in _collectItems())
        {
            item.IsActive = index >= first && index < first + PageSize;
            index++;
        }
    }

    protected override void OnGroupChanged()
    {
        _showPage();
        base.OnGroupChanged();
    }

    protected override void OnInit(Drawer d)
    {
        _showPage();
        base.OnInit(d);
    }

    protected override void OnUpdate(Drawer d)
    {
        _showPage();
        base.OnUpdate(d);
    }
}
