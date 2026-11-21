# GameObject

### Notes
* **Base entity class** for all scene and UI objects in `microgl`, implementing `IDisposable`.
* **Supports hierarchical 2D transforms** — the global transform is computed as `parent * Transform`, kept pure with no pivot baked in.
* **`GlobalTransform` is stored on the object** — it is refreshed on every `Init` / `Update` / `Draw` call and whenever the local `Transform` changes, so `GlobalPosition`, `GlobalRotation`, `GlobalScale`, `ToLocal` and `ToGlobal` work without a `Drawer`. Until the first `Init` / `Update` / `Draw`, the parent is `Transform.Identity`.
* **`CenterPoint`** shifts the draw origin (pivot) without affecting the global position. The offset is applied only at render time via an internal render transform.
* **`DrawerScope`** (internal) automatically saves and restores `d.Transform`, `d.WorldTransform`, and `d.BlendColor` around each `OnDraw`/`OnUpdate` call, so the caller's draw state is never clobbered.
* **Subclasses implement four hooks**: `OnInit`, `OnUpdate`, `OnDraw`, and `Dispose`.

### Usage
```csharp
// Local transform
obj.Position;  obj.Rotation;  obj.Scale;
obj.Transform;

// Global transform — parent from the last Init/Update/Draw call
obj.GlobalTransform;
obj.GlobalPosition;  obj.GlobalRotation;  obj.GlobalScale;

// Pivot & tint
obj.CenterPoint;
obj.BlendColor;

// Lifecycle
obj.IsAlive;
obj.Delete();
obj.IsActive;   // false skips Update and Draw but keeps the object in its group

// Coordinate space conversion
obj.ToLocal(Vector2 global);
obj.ToGlobal(Vector2 local);

// Check that object is displayed on screen. Uses GlobalPosition
obj.IsOnScreen(Drawer d, Vector2 size);

// Lifecycle methods — no-parent overloads use Transform.Identity as parent
// Use drawer.DrawObject(o) to draw something with relative coords 
obj.Init(Drawer d);
obj.Init(Drawer d, Transform? parent);
obj.Update(Drawer d);
obj.Update(Drawer d, Transform? parent);
obj.Draw(Drawer d);
obj.Draw(Drawer d, Transform? parent);
obj.Dispose();
```

### Example
```csharp
public class Box : GameObject
{
    protected override void OnInit(Drawer d)
    {
        // CenterPoint pivots drawing around the box center
        // without shifting the global position
        CenterPoint = new Vector2(16f, 16f);
    }

    protected override void OnUpdate(Drawer d)
    {
        var gpos = GlobalPosition;
    }

    protected override void OnDraw(Drawer d)
    {
        // d.Transform already includes position, rotation, scale, and CenterPoint
        d.Color = Color.Cyan;
        d.DrawRect(new Rectangle(0, 0, 32, 32));

        // Draw a child object — it inherits this object's WorldTransform
        d.DrawObject(childIcon);
    }

    public override void Dispose() { }
}

// Root usage (no parent — uses Transform.Identity)
var box = new Box { Position = new Vector2(100f, 100f) };
box.Init(d);
box.Update(d);
box.Draw(d);

// Child usage — pass parent's current world transform
box.Draw(d, parentWorldTransform);
```
