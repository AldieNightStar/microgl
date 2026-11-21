# Tutorial 4: UI layout with UITable and PagedList

In this tutorial you place UI elements in rows, columns and grids, and split a long list into pages.

You will use: [UITable](../GameObject/UI/UITable.md), [PagedList](../GameObject/UI/PagedList.md), [UIElement](../GameObject/UI/UIElement.md). More ready-made layouts are in [UITable Examples](../GameObject/UI/UITableExamples.md).

The snippets assume a loaded `font`, a `Drawer d`, and this helper:

```csharp
private static readonly Vector2 BUTTON_SIZE = new(140f, 36f);

private Button _makeButton(string text) => new(_font, Vector2.Zero, BUTTON_SIZE, text);
```

Remember to call `Init`, `Update`, `Draw` and `Dispose` on the outer table only. It runs everything inside.

### Step 1: Rows and columns

Think of a table as lines of text:

* `Add(element)` puts the element on the current row, next to the previous one.
* `NextRow()` starts a new row.
* `AddRow(a, b, c)` adds the elements and then starts a new row.

```csharp
var table = new UITable(new Vector2(40f, 40f));

table.Add(_makeButton("A")).Add(_makeButton("B")).NextRow()
     .Add(_makeButton("C"));
```

This gives `A B` on the first row and `C` below `A`.

### Step 2: Spacing, padding, alignment and background

```csharp
var panel = new UITable(d.ScreenSize / 2f, centerOrigin: true)
{
    Padding = 16f,
    Spacing = new Vector2(8f, 8f),
    HorizontalAlign = TextAlign.Center,
    VerticalAlign = VerticalAlign.Middle,
    BackgroundColor = Color.FromArgb(200, 30, 35, 48),
    CornerRadius = 10
};
```

* `Padding` is the space between the background edge and the content.
* `Spacing.X` is the gap between cells, `Spacing.Y` the gap between rows.
* `HorizontalAlign` and `VerticalAlign` place each element inside its cell.
* With `centerOrigin: true`, the position is the center of the table.

### Step 3: A grid

Set `Columns` and the table starts a new row by itself after that many elements.

```csharp
const int INVENTORY_COLUMNS = 4;

var inventory = new UITable(new Vector2(40f, 40f))
{
    Columns = INVENTORY_COLUMNS,
    Spacing = new Vector2(6f, 6f)
};

foreach (var item in items)
    inventory.Add(new Button(_font, Vector2.Zero, new Vector2(64f, 64f), icon: item.Icon));
```

### Step 4: A form that updates itself

Two columns line up because each column is as wide as its widest element. The table re-lays out every frame, so changing a label's text just works.

```csharp
var gold = new Label(_font, Vector2.Zero, "0", Color.Gold);

var stats = new UITable(new Vector2(20f, 20f)) { Spacing = new Vector2(24f, 4f) };
stats.AddRow(new Label(_font, Vector2.Zero, "Health"), new Label(_font, Vector2.Zero, "100"))
     .AddRow(new Label(_font, Vector2.Zero, "Gold"), gold);

gold.Text = "250";
```

For a table that never changes, set `AutoLayout = false` and call `Layout()` yourself after a change.

### Step 5: Keep the table inside a size

`MaxSize` limits the table. Every element in it gets the space of its cell, and labels or buttons that are too long are cut at the edge.

```csharp
var sidebar = new UITable(new Vector2(10f, 10f)) { MaxSize = new Vector2(200f, 400f) };
sidebar.AddRow(new Label(_font, Vector2.Zero, "This very long title is cut at 200 pixels"));
```

`MinSize` keeps the table at least that big, even when it is almost empty.

### Step 6: Paged lists

Rows that do not fit vertically still overflow. For long lists use a `PagedList`: it shows `pageSize` elements at a time and hides the rest.

```csharp
var levels = new PagedList(new Vector2(360f, 200f), pageSize: 4, centerOrigin: true)
{
    Columns = 1,
    Spacing = new Vector2(0f, 6f),
    Padding = 10f,
    BackgroundColor = Color.FromArgb(200, 30, 35, 48),
    CornerRadius = 8
};

foreach (var name in levelNames)
{
    var button = _makeButton(name);
    button.OnClick += () => _startLevel(name);
    levels.Add(button);
}
```

Add a pager row with previous and next buttons:

```csharp
var pageLabel = new Label(_font, Vector2.Zero, "");
var previous = new Button(_font, Vector2.Zero, new Vector2(40f, 32f), "<");
var next = new Button(_font, Vector2.Zero, new Vector2(40f, 32f), ">");
previous.OnClick += levels.PreviousPage;
next.OnClick += levels.NextPage;

var pager = new UITable(new Vector2(360f, 380f), centerOrigin: true) { Spacing = new Vector2(12f, 0f) };
pager.Add(previous).Add(pageLabel).Add(next);
```

Refresh the pager every frame:

```csharp
pageLabel.Text = $"{levels.Page + 1} / {levels.PageCount}";
previous.Enabled = levels.HasPreviousPage;
next.Enabled = levels.HasNextPage;
levels.Update(d);
pager.Update(d);
```

Set `Columns = 2` and `pageSize: 4` to get 2x2 pages, for example for an inventory.

### Step 7: Tables inside tables

A table is a UI element too, so it can be a cell of another table.

```csharp
var side = new UITable(Vector2.Zero).AddRow(_makeButton("Profile")).AddRow(_makeButton("Friends"));
var grid = new UITable(Vector2.Zero) { Columns = 2 };
grid.Add(_makeButton("Play")).Add(_makeButton("Shop")).Add(_makeButton("Options")).Add(_makeButton("Quit"));

var screen = new UITable(new Vector2(40f, 40f)) { Spacing = new Vector2(24f, 0f), VerticalAlign = VerticalAlign.Top };
screen.AddRow(side, grid);
```

### Next

[Tutorial 5: Tweens and animation](05-Tweens.md) makes the UI and the game move.
