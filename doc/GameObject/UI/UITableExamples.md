# UITable Examples

Common layouts built with [UITable](UITable.md). Every snippet assumes a loaded `font` and a `Drawer d`, and uses the same button size:
```csharp
var buttonSize = new Vector2(140f, 36f);
Button MakeButton(string text) => new Button(font, Vector2.Zero, buttonSize, text);
```

### Rules of thumb
* `Add(element)` puts the element in the current row. `NextRow()` starts a new row.
* `AddRow(a, b, c)` is `Add(a).Add(b).Add(c).NextRow()`.
* One element per row gives a vertical list. All elements in one row give a horizontal bar.
* `UniformColumns = true` (default) lines cells up into a grid. `false` lets every row size itself and centers it with `HorizontalAlign`.

### Vertical list
One element per row.
```csharp
var list = new UITable(new Vector2(40f, 40f))
{
    Spacing = new Vector2(0f, 8f)
};

list.AddRow(MakeButton("New Game"))
    .AddRow(MakeButton("Continue"))
    .AddRow(MakeButton("Settings"))
    .AddRow(MakeButton("Quit"));
```

### Vertical list from data
```csharp
var levels = new[] { "Forest", "Caves", "Castle" };
var list = new UITable(new Vector2(40f, 40f));

foreach (var level in levels)
{
    var button = MakeButton(level);
    button.OnClick += () => StartLevel(level);
    list.AddRow(button);
}
```

### Horizontal bar
All elements in one row.
```csharp
var toolbar = new UITable(new Vector2(20f, 20f))
{
    Spacing = new Vector2(8f, 0f)
};

toolbar.AddRow(MakeButton("Build"), MakeButton("Move"), MakeButton("Delete"));
```

### 2x2 menu
Two rows of two. `UniformColumns` keeps the columns aligned.
```csharp
var grid = new UITable(d.ScreenSize / 2f, centerOrigin: true)
{
    Spacing = new Vector2(12f, 12f),
    HorizontalAlign = TextAlign.Center
};

grid.AddRow(MakeButton("Play"), MakeButton("Shop"))
    .AddRow(MakeButton("Options"), MakeButton("Quit"));
```

### Grid of any size
`Columns` starts a new row after that many elements, so no `NextRow()` calls are needed.
```csharp
const int INVENTORY_COLUMNS = 3;
var inventory = new UITable(new Vector2(40f, 40f)) { Columns = INVENTORY_COLUMNS };

foreach (var item in items)
    inventory.Add(MakeButton(item.Name));
```

### Title above a grid
With `UniformColumns = false` every row is centered on its own, so a single title sits above a wider row.
```csharp
var menu = new UITable(d.ScreenSize / 2f, centerOrigin: true)
{
    Padding = 16f,
    Spacing = new Vector2(12f, 12f),
    HorizontalAlign = TextAlign.Center,
    UniformColumns = false,
    BackgroundColor = Color.FromArgb(200, 30, 35, 48),
    CornerRadius = 10
};

menu.AddRow(new Label(font, Vector2.Zero, "Main Menu", Color.Gold))
    .AddRow(MakeButton("Play"), MakeButton("Shop"))
    .AddRow(MakeButton("Options"), MakeButton("Quit"));
```

### Label and value form
Two columns: names on the left, values on the right.
```csharp
var stats = new UITable(new Vector2(20f, 20f))
{
    Spacing = new Vector2(24f, 4f)
};

stats.AddRow(new Label(font, Vector2.Zero, "Health"), new Label(font, Vector2.Zero, "100"))
     .AddRow(new Label(font, Vector2.Zero, "Gold"), new Label(font, Vector2.Zero, "250"))
     .AddRow(new Label(font, Vector2.Zero, "Level"), new Label(font, Vector2.Zero, "7"));
```

### Confirm dialog
A message row and a button row, centered on screen.
```csharp
var dialog = new UITable(d.ScreenSize / 2f, centerOrigin: true)
{
    Padding = new Vector2(24f, 16f),
    Spacing = new Vector2(12f, 16f),
    HorizontalAlign = TextAlign.Center,
    UniformColumns = false,
    BackgroundColor = Color.FromArgb(230, 20, 22, 30),
    CornerRadius = 12
};

dialog.AddRow(new Label(font, Vector2.Zero, "Quit the game?"))
      .AddRow(MakeButton("Yes"), MakeButton("No"));
```

### Nested tables
A table is a `UIElement`, so it can be a cell of another table. Here a side list sits next to a 2x2 grid.
```csharp
var side = new UITable(Vector2.Zero)
    .AddRow(MakeButton("Profile"))
    .AddRow(MakeButton("Friends"));

var grid = new UITable(Vector2.Zero)
    .AddRow(MakeButton("Play"), MakeButton("Shop"))
    .AddRow(MakeButton("Options"), MakeButton("Quit"));

var screen = new UITable(new Vector2(40f, 40f))
{
    Spacing = new Vector2(24f, 0f),
    VerticalAlign = VerticalAlign.Top
};

screen.AddRow(side, grid);
```

### Lifecycle
Only the outer table needs to be driven. It inits, updates, draws and disposes its children.
```csharp
menu.Init(d);
menu.Update(d);
menu.Draw(d);
menu.Dispose();
```
