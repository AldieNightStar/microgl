# GameObjectGroup

A `GameObject` that holds other game objects and runs them: it inits, updates, draws and disposes all of its children. Use one for your game world, for each screen, or to move several objects together.

New to this? See [Tutorial 2: Game objects and sprites](../Tutorials/02-GameObjects.md).

### Example
```csharp
private readonly GameObjectGroup _world = new();

public void Init(Drawer d)
{
    _world.Add(new Player());
    _world.Add(new Enemy());
    _world.Init(d);
}

public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt) => _world.Update(d);

public void Draw(Drawer d, float dt) => _world.Draw(d);

public void Dispose() => _world.Dispose();
```

### Notes
* **Children follow the group**: they are placed relative to the group, so moving, rotating or scaling the group moves all of them.
* **Adding is delayed**: `Add()` queues the object, and the group inits it on its next `Update()`. That makes it safe to add objects at any time, even from a child's `OnUpdate`.
* **Removing**: call `obj.Delete()` or `group.Remove(obj)`. The group disposes and removes the object on its next `Update()`. A queued object is removed and disposed right away.
* **`Clear()`** disposes all children at once.
* **Finding children**: `GetChildren()` returns the active list. `GetChildren<T>()` returns children of one type, queued ones included. `CollectChildren<T>(list)` does the same into a list you keep and allocates nothing, so prefer it in code that runs every frame.
* **`OnChildrenChanged`** fires once per `Update()` in which children were added or removed, and right away when `Remove()` drops a queued object or `Clear()` empties a non-empty group. `UITable` uses it to re-layout.
* **`Dispose()`** removes all `OnChildrenChanged` subscribers, then disposes every child.

### Usage
```csharp
new GameObjectGroup();
new GameObjectGroup(List<GameObject> objects);

group.Add(GameObject obj);
group.Remove(GameObject obj);
group.Clear();

group.GetChildren();                    // IReadOnlyList<GameObject>
group.GetChildren<T>();                 // IEnumerable<T>, queued included
group.CollectChildren<T>(List<T> list); // no allocation

group.OnChildrenChanged;
```

### Example: find and remove children
```csharp
private readonly List<Enemy> _enemies = new();

public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
{
    _world.Update(d);
    _world.CollectChildren(_enemies);

    foreach (var enemy in _enemies)
        if (enemy.Health <= 0) enemy.Delete();
}
```

### Example: move objects together
```csharp
var squad = new GameObjectGroup { Position = new Vector2(400f, 100f) };
squad.Add(new Enemy { Position = new Vector2(0f, 0f) });
squad.Add(new Enemy { Position = new Vector2(40f, 0f) });
_world.Add(squad);

squad.Position += new Vector2(10f, 0f);
```
