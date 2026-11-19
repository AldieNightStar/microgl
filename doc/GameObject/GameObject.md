# GameObject

### Notes
* **Base entity class** for all scene and UI objects in `microgl`, implementing `IDisposable`.
* **Supports hierarchical 2D transforms** (`Position`, `Rotation`, `Scale`, `CenterPoint`) with automatic `GlobalTransform` propagation from parent objects.
* **Provides local and global transformation helpers** (`Move`, `Rotate`, `Resize`, `ToLocal`, `ToGlobal`) alongside `BlendColor` tinting and `IsAlive` lifecycle state.
* **Subclasses implement four lifecycle hooks**: `OnInit`, `OnUpdate`, `OnDraw`, and `Dispose`.

### Usage
```csharp
// Core transform, origin, tint, and alive state properties
obj.Position; obj.GlobalPosition;
obj.Rotation; obj.GlobalRotation;
obj.Scale; obj.GlobalScale;
obj.CenterPoint;
obj.Transform; obj.GlobalTransform;
obj.BlendColor;
obj.IsAlive;

// Relative and global transform helpers + coordinate space conversion
obj.Move(Vector2 vec);
obj.Rotate(float rad);
obj.Resize(Vector2 scale);
obj.GlobalMove(Vector2 vec);
obj.GlobalRotate(float rad);
obj.ToLocal(Vector2 global);
obj.ToGlobal(Vector2 local);
obj.Delete();

// Lifecycle methods (called at root or with a parent Transform)
obj.Init(Drawer d);
obj.Init(Drawer d, Transform parentTransform);
obj.Update(Drawer d);
obj.Update(Drawer d, Transform parentTransform);
obj.Draw(Drawer d);
obj.Draw(Drawer d, Transform parentTransform);
obj.Dispose();
```

### Example
```csharp
public class Box : GameObject
{
    protected override void OnInit(Drawer d)
    {
        CenterPoint = new Vector2(16f, 16f);
    }

    protected override void OnUpdate(Drawer d)
    {
        Rotate(0.02f);
    }

    protected override void OnDraw(Drawer d)
    {
        d.DrawRect(new Rectangle(0, 0, 32, 32));
    }

    public override void Dispose()
    {
    }
}

// Root usage
var box = new Box { Position = new Vector2(100f, 100f), BlendColor = Color.Cyan };
box.Init(d);
box.Update(d);
box.Draw(d);
box.Dispose();

// Draw as child
box.Draw(d, this.Global);
box.Draw(d, this.Global);
```
