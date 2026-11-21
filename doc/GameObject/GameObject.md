# GameObject

The base class for everything in your game: players, enemies, bullets, UI widgets. Inherit from it, fill in four methods, and it handles position, rotation, scale and parent-child movement for you.

New to this? See [Tutorial 2: Game objects and sprites](../Tutorials/02-GameObjects.md).

### Example
```csharp
public class Box : GameObject
{
    private const int SIZE = 32;
    private const float SPIN_SPEED = 2f;

    public Box(Vector2 position)
    {
        Position = position;
        CenterPoint = new Vector2(SIZE / 2f, SIZE / 2f);
    }

    protected override void OnInit(Drawer d) { }

    protected override void OnUpdate(Drawer d)
    {
        Rotation += SPIN_SPEED * Time.DeltaTime;
    }

    protected override void OnDraw(Drawer d)
    {
        d.Color = Color.Cyan;
        d.DrawRect(new Rectangle(0, 0, SIZE, SIZE));
    }

    public override void Dispose() { }
}

// In IGame
var box = new Box(new Vector2(100f, 100f));
box.Init(d);      // in Init
box.Update(d);    // in Update
box.Draw(d);      // in Draw
box.Dispose();    // in Dispose
```

Most of the time you add objects to a [GameObjectGroup](GameObjectGroup.md) instead, which calls these four methods for all of its children.

### Notes
* **Four methods to fill in**: `OnInit` (once), `OnUpdate` and `OnDraw` (every frame), and `Dispose`. You call `Init`, `Update` and `Draw`, never the `On...` methods.
* **Draw at `(0, 0)`**: in `OnDraw` the drawer is already moved, rotated and scaled to the object.
* **Frame time**: `OnUpdate` gets no `dt`, use `Time.DeltaTime`.
* **`CenterPoint` is the pivot**: rotation and scale happen around it, and `Position` refers to it. It does not move the object.
* **`BlendColor` tints the object**: it is set as `drawer.BlendColor` during `OnDraw`, so textures, labels and buttons are tinted. Shapes drawn with `drawer.Color` are not.
* **Local and global**: `Position`, `Rotation` and `Scale` are relative to the parent. `GlobalPosition`, `GlobalRotation` and `GlobalScale` are on screen, and can also be set. They use the parent from the last `Init`, `Update` or `Draw`, and `Transform.Identity` before that.
* **Delete and hide**: `Delete()` sets `IsAlive = false`, and the owning group disposes and removes the object on its next update. `IsActive = false` skips `Update` and `Draw` but keeps the object.
* **The drawer state is restored** after `OnUpdate` and `OnDraw`, so an object never changes how the caller draws.

### Usage
```csharp
// Local transform, relative to the parent
obj.Position;  obj.Rotation;  obj.Scale;
obj.Transform;

// Global transform, on screen
obj.GlobalTransform;
obj.GlobalPosition;  obj.GlobalRotation;  obj.GlobalScale;

// Pivot and tint
obj.CenterPoint;
obj.BlendColor;

// State
obj.IsAlive;
obj.Delete();
obj.IsActive;

// Coordinate conversion
obj.ToLocal(Vector2 global);
obj.ToGlobal(Vector2 local);

// Lifecycle, parent defaults to Transform.Identity
obj.Init(Drawer d, Transform? parent = null);
obj.Update(Drawer d, Transform? parent = null);
obj.Draw(Drawer d, Transform? parent = null);
obj.Dispose();

// Hooks to override
protected abstract void OnInit(Drawer d);
protected abstract void OnUpdate(Drawer d);
protected abstract void OnDraw(Drawer d);
public abstract void Dispose();
```

### Example: child objects
Inside `OnDraw`, `d.DrawObject` draws another object relative to this one, so it follows its parent.
```csharp
protected override void OnDraw(Drawer d)
{
    d.Color = Color.Cyan;
    d.DrawRect(new Rectangle(0, 0, 32, 32));
    d.DrawObject(_icon, new Vector2(8f, -16f));
}
```
