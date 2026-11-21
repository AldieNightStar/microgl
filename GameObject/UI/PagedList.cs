using System.Numerics;

namespace microgl;

public class PagedList : UITable
{
    private const int DEFAULT_PAGE_SIZE = 5;

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

    public int ItemCount => Items().Count();
    public int PageCount => Math.Max(1, (ItemCount + PageSize - 1) / PageSize);
    public bool HasNextPage => Page < PageCount - 1;
    public bool HasPreviousPage => Page > 0;

    public PagedList(Vector2 position, int pageSize = DEFAULT_PAGE_SIZE, GameObjectGroup? group = null, bool centerOrigin = false)
        : base(position, group, centerOrigin)
    {
        PageSize = pageSize;
    }

    public void NextPage() => Page++;
    public void PreviousPage() => Page--;

    private IEnumerable<UIElement> Items() => Group.GetChildren<UIElement>().Where(item => item.IsAlive);

    private void ShowPage()
    {
        int first = Page * PageSize;
        int index = 0;

        foreach (var item in Items())
        {
            item.IsActive = index >= first && index < first + PageSize;
            index++;
        }
    }

    protected override void OnInit(Drawer d)
    {
        ShowPage();
        base.OnInit(d);
    }

    protected override void OnUpdate(Drawer d)
    {
        ShowPage();
        base.OnUpdate(d);
    }
}
