# ClickableArea

An invisible area that reports hover and clicks. Use it to make anything clickable, such as a sprite or a region of a map.

New to this? See [Tutorial 3: Buttons and menus](../../Tutorials/03-ButtonsAndMenus.md).

### Notes
* **Invisible interactive hitbox** inheriting from `UIElement` that tracks left-mouse hover, press, and click events without rendering anything.
* **Supports hierarchical transforms** via `UIElement.ContainsPoint()`, working accurately even when moved, rotated, scaled, or nested inside a parent container.
* **Only reacts while it is updated**: an area (or a `Button`) that missed the last frame's `Update`, for example because it is inactive or on another `PagedList` page, ignores the mouse.
* **Can be initialized from a `Rectangle`** (top-left origin by default) or from `(Vector2 position, Vector2 size)` (centered origin by default).

### Usage
```csharp
// Create a clickable area from a Rectangle (centerOrigin defaults to false) or position/size (defaults to true)
new ClickableArea(Rectangle rect, bool centerOrigin = false);
new ClickableArea(Vector2 position, Vector2 size, bool centerOrigin = true);

// Current hover/press state and toggle flag
area.IsHovered; area.IsPressed; area.Enabled;

// Mouse interaction events (triggered by left mouse button)
area.OnClick; area.OnHoverEnter; area.OnHoverExit;

// Lifecycle methods (Init subscribes to Mouse events; Dispose unsubscribes)
area.Init(Drawer d);
area.Update(Drawer d);
area.Draw(Drawer d);
area.Dispose();
```

### Example
```csharp
var area = new ClickableArea(new Rectangle(100, 100, 160, 48));
area.OnHoverEnter += () => Console.WriteLine("Hover enter");
area.OnClick += () => Console.WriteLine("Clicked!");

area.Init(d);
area.Update(d);
area.Dispose();
```
