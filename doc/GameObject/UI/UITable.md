# UITable

### Notes
* **Layout container** inheriting from `UIElement`. It does not store its own element list: every child lives in a `GameObjectGroup` (`table.Group`), and the table only arranges them.
* **Accepts or creates a group**: pass an existing `GameObjectGroup` to the constructor, or let the table create one. Add, remove or clear objects through the group as usual; the table disposes the group with itself.
* **Rows via `NextRowMarker`**: `NextRow()` adds a `NextRowMarker` object to the group. Every `UIElement` after a marker goes to the next row. Repeated or leading markers do not create empty rows. Non-`UIElement` objects in the group are updated and drawn but not laid out.
* **Column limit**: with `Columns` above 0 a new row starts automatically after that many elements, so a grid needs no `NextRow()` calls. `0` (default) means no limit.
* **Inactive objects** (`IsActive = false`) are skipped by the layout, so hidden elements leave no gap.
* **Sizing**: each row is as tall as its tallest element. With `UniformColumns` (default) each column is as wide as its widest element across all rows, like a GDX `Table` or Godot `GridContainer`. With `UniformColumns = false` every row flows independently and the whole row is aligned by `HorizontalAlign`.
* **Alignment**: `HorizontalAlign` (`TextAlign`) and `VerticalAlign` place each element inside its cell.
* **Auto layout**: with `AutoLayout` (default) the table calls `Layout()` every `Update()`, so text or size changes are picked up immediately. Turn it off and call `Layout()` yourself for static tables.
* **Table size** = content + `Padding`, clamped by `MinSize`. `MaxSize` is unlimited by default.
* **Children are positioned in the table's local space** (top-left is `0,0`), even when `centerOrigin` is true.
* **Background**: `BackgroundTexture` (drawn tinted by `BlendColor`) takes priority over `BackgroundColor`. `CornerRadius` rounds the corners of either.

### Insets
`Padding` is an `Insets` (`Left`, `Top`, `Right`, `Bottom`). A `float` converts to equal insets and a `Vector2` to horizontal/vertical insets.
```csharp
table.Padding = 12f;
table.Padding = new Vector2(16f, 8f);
table.Padding = new Insets(4f, 8f, 4f, 16f);
table.Padding = Insets.Symmetric(16f, 8f);
```

### Usage
```csharp
new UITable(Vector2 position, GameObjectGroup? group = null, bool centerOrigin = false);

table.Group;
table.Padding;            // Insets
table.Spacing;            // Vector2 gap between cells (X) and rows (Y), default 6
table.HorizontalAlign;    // TextAlign, default Left
table.VerticalAlign;      // VerticalAlign, default Middle
table.UniformColumns;     // default true
table.Columns;            // elements per row before wrapping, 0 = no limit
table.AutoLayout;         // default true
table.BackgroundTexture;
table.BackgroundColor;
table.CornerRadius;
table.RowCount;
table.ColumnCount;

table.Add(UIElement element);
table.AddRow(params UIElement[] elements);   // adds the elements, then NextRow()
table.NextRow();
table.Layout();
```

### Example
```csharp
var table = new UITable(new Vector2(360f, 240f), centerOrigin: true)
{
    BackgroundColor = Color.FromArgb(200, 30, 35, 48),
    CornerRadius = 10,
    Padding = new Vector2(16f, 12f),
    Spacing = new Vector2(8f, 8f),
    HorizontalAlign = TextAlign.Center,
    UniformColumns = false
};

var play = new Button(font, Vector2.Zero, new Vector2(120f, 36f), "Play");
var quit = new Button(font, Vector2.Zero, new Vector2(120f, 36f), "Quit");

table
    .AddRow(new Label(font, Vector2.Zero, "Menu", Color.Gold))
    .Add(play)
    .Add(quit);

table.Init(d);
table.Update(d);
table.Draw(d);

table.Group.Remove(quit);
table.Dispose();
```

More layouts (vertical list, 2x2 menu, grids, forms, dialogs, nested tables): [UITable Examples](UITableExamples.md).
