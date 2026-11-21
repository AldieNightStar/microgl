# UIElement

The base class of every UI widget. It adds a size, size limits and bounds to `GameObject`. Inherit from it to make your own widget.

New to this? See [Tutorial 4: UI layout with UITable and PagedList](../../Tutorials/04-UILayout.md).

### Notes
* **Abstract base class** for all UI widgets in `microgl`, inheriting from `GameObject`.
* **Size never exceeds its limit**: `Size` is clamped between `MinSize` (default `0, 0`) and `SizeLimit`, which is the smaller of `MaxSize` (default `1000, 1000`) and `AvailableSize` (unlimited by default). If `MinSize` is larger than the limit, the limit wins. `OnResized()` runs whenever the clamped size changes.
* **The requested size is kept**: setting `Size` stores the value you asked for. When `MaxSize` or `AvailableSize` later grows, the element grows back toward it, so an element can change any of these values in `OnInit`, `OnUpdate` or any other method at any time.
* **`AvailableSize` is set by containers**: `UITable` and `PagedList` give each child the space of its cell, and `Button` gives its label the space inside the button. Your own `MaxSize` stays untouched.
* **Manages origin and bounding boxes**: `CenterOrigin` keeps `CenterPoint` synced to `Size / 2`, while `LocalBounds`, `Bounds`, and `GlobalBounds` expose exact rectangles.
* **`ContainsPoint` and `GlobalBounds` use the stored `GlobalTransform`**, so they work anywhere, including mouse-event handlers.

### Usage
```csharp
// Base constructors for subclasses (by position/size or by Rectangle bounds)
protected UIElement();
protected UIElement(Vector2 position, Vector2 size, bool centerOrigin = false);
protected UIElement(Rectangle bounds, bool centerOrigin = false);

// Size, clamping limits (default MaxSize is 1000x1000), and automatic center origin toggle
element.Size;
element.MinSize;
element.MaxSize;
element.AvailableSize;    // limit set by the parent container, unlimited by default
element.SizeLimit;        // Min(MaxSize, AvailableSize), the size never goes above it
element.CenterOrigin;

// Bounding rectangles in local, parent, and global world/screen space
element.LocalBounds;
element.Bounds;
element.GlobalBounds;

// Check if a screen-space coordinate lies within this element's transformed bounds
element.ContainsPoint(Vector2 screenPos);

// Virtual hook called whenever Size changes
protected virtual void OnResized();
```

### Example
```csharp
public class Panel : UIElement
{
    public Panel(Vector2 pos, Vector2 size) : base(pos, size, centerOrigin: true)
    {
        MaxSize = new Vector2(800f, 600f);
    }

    protected override void OnInit(Drawer d) { }
    protected override void OnUpdate(Drawer d) { }

    protected override void OnDraw(Drawer d)
    {
        d.DrawRect(LocalBounds);
    }

    public override void Dispose() { }
}

var panel = new Panel(new Vector2(360f, 240f), new Vector2(200f, 120f));

bool hit = panel.ContainsPoint(Mouse.Position);
var worldRect = panel.GlobalBounds;
```
