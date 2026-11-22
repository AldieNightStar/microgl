# GameObject

The base class for things in your game: players, enemies, bullets. Inherit from it and fill in four methods. It handles position, rotation, scale and parent-child movement.

### Example
```csharp
public class Box : GameObject
{
    private const int Size = 32;
    private const float SpinSpeed = 2f;

    public Box(Vector2 position)
    {
        Position = position;
        CenterPoint = new Vector2(Size / 2f, Size / 2f);
    }

    protected override void OnInit(Drawer d) { }

    protected override void OnUpdate(Drawer d)
    {
        Rotation += SpinSpeed * Time.DeltaTime;
    }

    protected override void OnDraw(Drawer d)
    {
        d.Color = Color.Cyan;
        d.DrawRect(new Rectangle(0, 0, Size, Size));
    }

    public override void Dispose() { }
}

var box = new Box(new Vector2(100f, 100f));
box.Init(d);
box.Update(d);
box.Draw(d);
box.Dispose();
```

Most of the time you add objects to a [GameObjectGroup](GameObjectGroup.md), which calls these methods for all of its children.

### Notes
* **Four methods to fill in**: `OnInit` (once), `OnUpdate` and `OnDraw` (every frame), and `Dispose`. You call `Init`, `Update` and `Draw`, never the `On...` methods.
* **Draw at `(0, 0)`**: in `OnDraw` the drawer is already moved, rotated and scaled to the object.
* **Frame time**: `OnUpdate` gets no `dt`. Use `Time.DeltaTime`.
* **`CenterPoint` is the pivot**: rotation and scale happen around it, and `Position` is where that pivot sits.
* **`BlendColor` tints textures**: it is set as the drawer's blend color during `OnDraw`. Shapes drawn with `drawer.Color` are not tinted.
* **Global values**: `GlobalPosition`, `GlobalRotation` and `GlobalScale` are on screen, and can be set. They use the parent passed to the last `Init`, `Update` or `Draw`, or `Transform.Identity` before that.
* **Removing**: set `IsAlive = false`. `Update` and `Draw` skip dead objects, and the owning group disposes and removes them on its next update.
* **Drawer state is restored** after each `On...` method, so an object never changes how the caller draws.

### Usage
```csharp
obj.Position;
obj.Rotation;
obj.RotationDegrees;
obj.Scale;
obj.Skew;
obj.CenterPoint;
obj.BlendColor;

obj.Transform;
obj.GlobalTransform;
obj.GlobalPosition;
obj.GlobalRotation;
obj.GlobalRotationDegrees;
obj.GlobalScale;

obj.Right;
obj.Down;
obj.MoveLocal(Vector2 delta);
obj.LookAt(Vector2 worldTarget);
obj.ToLocal(Vector2 worldPos);
obj.ToGlobal(Vector2 localPos);

obj.IsAlive;

obj.Init(Drawer d, Transform? parent = null);
obj.Init(GameObject parent, Drawer d);
obj.Update(Drawer d, Transform? parent = null);
obj.Update(GameObject parent, Drawer d);
obj.Draw(Drawer d, Transform? parent = null);
obj.Draw(GameObject parent, Drawer d);
obj.Dispose();

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
