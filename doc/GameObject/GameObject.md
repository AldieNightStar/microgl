# GameObject

### Notes
* **Base entity class** for all scene and UI objects in `microgl`, implementing `IDisposable`.
* **Supports hierarchical 2D transforms** — `GlobalTransform` is always `parent * Transform`, kept pure with no pivot baked in.
* **`CenterPoint`** shifts the draw origin (pivot) without affecting `GlobalPosition` or `GlobalTransform`. The offset is applied only at render time via an internal render transform.
* **`DrawerScope`** (internal) automatically saves and restores `d.Transform`, `d.WorldTransform`, and `d.BlendColor` around each `OnDraw`/`OnUpdate` call, so the caller's draw state is never clobbered.
* **Subclasses implement four hooks**: `OnInit`, `OnUpdate`, `OnDraw`, and `Dispose`.

### Usage
```csharp
// Local transform
obj.Position;  obj.Rotation;  obj.Scale;
obj.Transform;

// World transform (read-only, always parent * local)
obj.GlobalPosition;  obj.GlobalRotation;  obj.GlobalScale;
obj.GlobalTransform;

// Pivot & tint
obj.CenterPoint;
obj.BlendColor;

// Lifecycle
obj.IsAlive;
obj.Delete();

// Relative transform mutators
obj.Move(Vector2 delta);
obj.Rotate(float rad);
obj.Resize(Vector2 scale);
obj.GlobalMove(Vector2 delta);
obj.GlobalRotate(float rad);

// Coordinate space conversion
obj.ToLocal(Vector2 global);
obj.ToGlobal(Vector2 local);

// Lifecycle methods — no-parent overloads use Transform.Identity as parent
obj.Init(Drawer d);
obj.Init(Drawer d, Transform parent);
obj.Update(Drawer d);
obj.Update(Drawer d, Transform parent);
obj.Draw(Drawer d);
obj.Draw(Drawer d, Transform parent);
obj.Dispose();
```

### Example
```csharp
public class Box : GameObject
{
    protected override void OnInit(Drawer d)
    {
        // CenterPoint pivots drawing around the box center
        // without shifting GlobalPosition
        CenterPoint = new Vector2(16f, 16f);
    }

    protected override void OnUpdate(Drawer d)
    {
        Rotate(0.02f);
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

// Child usage — pass parent's GlobalTransform
box.Draw(d, parent.GlobalTransform);
```
