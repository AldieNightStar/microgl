# GameObject

### Notes
* **Base entity class** for all scene and UI objects in `microgl`, implementing `IDisposable`.
* **Supports hierarchical 2D transforms** — the global transform is computed as `parent * Transform`, kept pure with no pivot baked in.
* **Global transform methods require a `Drawer`** — `GetGlobalPosition`, `GetGlobalRotation`, `GetGlobalScale`, and their setters are methods that take `Drawer d` and are only valid inside `OnUpdate` / `OnDraw`.
* **`CenterPoint`** shifts the draw origin (pivot) without affecting the global position. The offset is applied only at render time via an internal render transform.
* **`DrawerScope`** (internal) automatically saves and restores `d.Transform`, `d.WorldTransform`, and `d.BlendColor` around each `OnDraw`/`OnUpdate` call, so the caller's draw state is never clobbered.
* **Subclasses implement four hooks**: `OnInit`, `OnUpdate`, `OnDraw`, and `Dispose`.

### Usage
```csharp
// Local transform
obj.Position;  obj.Rotation;  obj.Scale;
obj.Transform;

// Global transform — requires Drawer, only valid inside OnUpdate/OnDraw
obj.GetGlobalPosition(Drawer d);
obj.SetGlobalPosition(Drawer d, Vector2 value);
obj.GetGlobalRotation(Drawer d);
obj.SetGlobalRotation(Drawer d, float value);
obj.GetGlobalScale(Drawer d);
obj.SetGlobalScale(Drawer d, Vector2 value);

// Pivot & tint
obj.CenterPoint;
obj.BlendColor;

// Lifecycle
obj.IsAlive;
obj.Delete();

// Coordinate space conversion (requires Drawer, only valid inside OnUpdate/OnDraw)
obj.ToLocal(Drawer d, Vector2 global);
obj.ToGlobal(Drawer d, Vector2 local);

// Lifecycle methods — no-parent overloads use Transform.Identity as parent
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
        // Global helpers are only safe here (inside the DrawerScope)
        var gpos = GetGlobalPosition(d);
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
