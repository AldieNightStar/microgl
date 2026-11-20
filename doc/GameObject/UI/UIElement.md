# UIElement

### Notes
* **Abstract base class** for all UI widgets in `microgl`, inheriting from `GameObject`.
* **Enforces size bounds** by clamping `Size` between `MinSize` (default `0, 0`) and `MaxSize` (default `1000, 1000`), triggering `OnResized()` on changes.
* **Manages origin and bounding boxes**: `CenterOrigin` keeps `CenterPoint` synced to `Size / 2`, while `LocalBounds`, `Bounds`, and `GetGlobalBounds(Drawer d)` expose exact rectangles.
* **`ToGlobal`, `ToLocal`, `ContainsPoint`, and `GetGlobalBounds` all require a `Drawer`** — they read the cached world transform and are only accurate inside `OnUpdate` / `OnDraw` or mouse-event handlers that store the drawer.

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
element.GetGlobalBounds(Drawer d);

// Coordinate space conversion (requires Drawer)
element.ToGlobal(Drawer d, Vector2 local);
element.ToLocal(Drawer d, Vector2 global);

// Check if a screen-space coordinate lies within this element's transformed bounds
element.ContainsPoint(Drawer d, Vector2 screenPos);

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

// ContainsPoint and GetGlobalBounds require the Drawer from Update/Draw
// (or stored from the last Update call)
bool hit = panel.ContainsPoint(d, Mouse.Position);
var worldRect = panel.GetGlobalBounds(d);
```
