# PagedList

### Notes
* **Paged container** inheriting from `UITable`. It shows `PageSize` elements of its `Group` at a time and lays them out with all the usual `UITable` settings (`Columns`, `Padding`, `Spacing`, alignment, background).
* **Group-based**: add, remove or clear elements through `list.Group` (or `Add` / `AddRow`). The list never copies them.
* **Hidden elements stay in the group**: elements outside the current page get `IsActive = false`, so they are not updated, drawn, laid out or clickable, and keep their state.
* **Group events**: when `Group.OnChildrenChanged` fires, the list refreshes the visible page and calls `Layout()`.
* **`Page` is clamped** to `0 .. PageCount - 1`, also when elements are removed. `PageCount` is at least 1.
* **Grid pages**: set `Columns` to wrap the page into a grid, e.g. `PageSize = 4, Columns = 2` gives 2x2 pages. Without `Columns` every element goes into one row, so use `Columns = 1` for a vertical list.
* **Size follows the page**: a short last page makes the list smaller. Set `MinSize` to keep a fixed size, and `MaxSize` to limit the list and every item on the page (see `UITable`).
* **`NextRow()` markers are not counted** as items. Prefer `Columns` inside a paged list.

### Usage
```csharp
new PagedList(Vector2 position, int pageSize = 5, GameObjectGroup? group = null, bool centerOrigin = false);

list.PageSize;          // elements per page, at least 1
list.Page;              // current page, from 0
list.PageCount;
list.ItemCount;
list.HasNextPage;
list.HasPreviousPage;

list.NextPage();
list.PreviousPage();

// Everything from UITable: Group, Columns, Padding, Spacing, alignment, background, Add, Layout...
```

### Example: vertical list with page buttons
```csharp
var list = new PagedList(new Vector2(360f, 200f), pageSize: 4, centerOrigin: true)
{
    Columns = 1,
    Spacing = new Vector2(0f, 6f),
    Padding = 10f,
    BackgroundColor = Color.FromArgb(200, 30, 35, 48),
    CornerRadius = 8
};

foreach (var levelName in levelNames)
    list.Add(new Button(font, Vector2.Zero, new Vector2(200f, 32f), levelName));

var pageLabel = new Label(font, Vector2.Zero, "");
var prev = new Button(font, Vector2.Zero, new Vector2(40f, 32f), "<");
var next = new Button(font, Vector2.Zero, new Vector2(40f, 32f), ">");
prev.OnClick += list.PreviousPage;
next.OnClick += list.NextPage;

var pager = new UITable(new Vector2(360f, 380f), centerOrigin: true) { Spacing = new Vector2(12f, 0f) };
pager.Add(prev).Add(pageLabel).Add(next);

// Update
pageLabel.Text = $"{list.Page + 1} / {list.PageCount}";
prev.Enabled = list.HasPreviousPage;
next.Enabled = list.HasNextPage;
list.Update(d);
pager.Update(d);
```

### Example: 2x2 inventory pages
```csharp
var inventory = new PagedList(new Vector2(40f, 40f), pageSize: 4) { Columns = 2, Spacing = new Vector2(8f, 8f) };

foreach (var item in items)
    inventory.Add(new Button(font, Vector2.Zero, new Vector2(64f, 64f), icon: item.Icon));

inventory.NextPage();
```
