# UITable

### Notes
* **Row-based layout container** inheriting from `UIElement` that arranges child `UIElement` items horizontally in rows and advances vertically via `NextLine()`.
* **Locks child bounds and positions**: computes each element's cell rectangle during `Layout()` and enforces its position on every `Update()` while propagating the current world transform.
* **`Add(Drawer d, UIElement element)` requires a `Drawer`** — if called after `Init`, the element is immediately initialized with the stored drawer; before `Init` the element is registered and initialized during `OnInit`.
* **Supports optional background styling** using either a `BackgroundTexture` or a solid `BackgroundColor`, plus configurable `Padding` and `Spacing`.

### Usage
```csharp
// Create a table at position with optional centerOrigin
new UITable(Vector2 position, bool centerOrigin = false);

// Optional background texture or solid color, cell spacing (default 6f), and outer padding
table.BackgroundTexture;
table.BackgroundColor;
table.Spacing;
table.Padding;

// Fluent methods to add elements to the current row and start a new row
table.Add(Drawer d, UIElement element);
table.NextLine();

// Recalculate cell bounds and table Size/MaxSize (call after changing child sizes or text)
table.Layout();

// Standard UIElement lifecycle methods (propagated to all child elements)
table.Init(Drawer d);
table.Update(Drawer d);
table.Draw(Drawer d);
table.Dispose();
```

### Example
```csharp
var table = new UITable(new Vector2(360f, 240f), centerOrigin: true)
{
    BackgroundColor = Color.FromArgb(200, 30, 35, 48),
    Padding = new Vector2(16f, 12f),
    Spacing = 8f
};

table
    .Add(d, new Label(font, Vector2.Zero, "Menu", Color.Gold))
    .NextLine()
    .Add(d, new Button(font, Vector2.Zero, new Vector2(120f, 36f), "Play"))
    .Add(d, new Button(font, Vector2.Zero, new Vector2(120f, 36f), "Quit"));

table.Init(d);
table.Update(d);
table.Draw(d);
table.Dispose();
```
