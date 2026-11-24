# GameObjectGroup

A `GameObject` that holds other game objects and runs them: it inits, updates, draws and disposes all of its children. Use one for your game world, for each screen, or to move several objects together.

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
* **Adding is delayed**: `Add` queues the object, and the group inits it on its next `Update`. `Add` returns `false` only while the group is starting queued objects.
* **Removing**: `group.Remove(obj)` or `obj.IsAlive = false`. An object in the list is disposed and removed on the group's next `Update`. A queued object is disposed and removed right away.
* **`Clear()`** marks all children dead (`IsAlive = false`) and disposes them now, so tweens that target them stop.
* **Finding children**: `GetChildren()` returns a read-only view of the active list. `CollectChildren<T>(list)` fills a list you pass in with children of one type, queued ones included. It clears that list first.
* **`OnChildrenChanged`** fires after an `Update` in which children were added or removed, and right away when `Remove` drops a queued object or `Clear` empties a non-empty group.
* **`Dispose()`** removes all `OnChildrenChanged` subscribers, then disposes every child.

### Usage
```csharp
new GameObjectGroup();
new GameObjectGroup(List<GameObject> objects);

group.Add(GameObject obj);
group.Remove(GameObject obj);
group.Clear();

group.GetChildren();
group.CollectChildren<T>(List<T> target);

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
        if (enemy.Health <= 0) enemy.IsAlive = false;
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
