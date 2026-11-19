# GameObjectGroup

### Notes
* **Composite container** inheriting from `GameObject` that manages, updates, draws, and disposes a collection of child `GameObject` instances.
* **Propagates parent transforms** (`GlobalTransform`) to all children, allowing an entire group to be moved, rotated, or scaled together.
* **Safely handles dynamic additions and removals**: new objects queued via `Add()` are initialized on the next `Update()`, while objects with `IsAlive == false` are automatically disposed and removed.

### Usage
```csharp
// Create an empty group or wrap an existing list of GameObject instances
new GameObjectGroup();
new GameObjectGroup(List<GameObject> objects);

// Queue a child GameObject to be added and initialized on the next Update
group.Add(GameObject obj);

// Standard GameObject lifecycle methods propagated to all children
group.Init(Drawer d);
group.Update(Drawer d);
group.Draw(Drawer d);
group.Dispose();
```

### Example
```csharp
var group = new GameObjectGroup();
group.Position = new Vector2(200f, 150f);

group.Add(new Box { Position = new Vector2(0f, 0f) });
group.Add(new Box { Position = new Vector2(40f, 0f) });

group.Init(d);
group.Update(d);
group.Draw(d);
group.Dispose();
```
