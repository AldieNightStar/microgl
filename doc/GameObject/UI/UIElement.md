# UIElement

### Notes
* **Abstract base class** for all UI widgets in `microgl`, inheriting from `GameObject`.
* **Enforces size bounds** by clamping `Size` between `MinSize` (default `0, 0`) and `MaxSize` (default `1000, 1000`), triggering `OnResized()` on changes.
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
