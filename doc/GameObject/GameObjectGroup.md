# GameObjectGroup

A `GameObject` that owns and manages a collection of child objects.

### Notes
* **Composite container** inheriting from `GameObject` that manages, updates, draws, and disposes a collection of child `GameObject` instances.
* **Propagates parent transforms** to all children, allowing an entire group to be moved, rotated, or scaled together.
* **Safely handles dynamic additions and removals**: new objects queued via `Add()` are initialized on the next `Update()`, while objects with `IsAlive == false` are automatically disposed and removed.
* **`OnChildrenChanged` event** fires when the child list changes: once per `Update()` in which queued objects were added or dead objects were removed, and right away when `Remove()` drops a queued object or `Clear()` empties a non-empty group. Containers like `UITable` use it to re-layout.
* **Dispose** removes all `OnChildrenChanged` subscribers first, then disposes every child.

### Usage
```csharp
// Create new Group
var group = new GameObjectGroup();

// Add new object inside
group.Add(new Enemy());
group.Add(new Coin());

// Get children
foreach (var child in group.GetChildren())
    Debug.WriteLine(child);

// Get specific type of children
foreach (var enemy in group.GetChildren<Enemy>())
    enemy.Alert();

// Addition & Removeal
group.Add(boss);
group.Remove(boss);

// Wipe all objects inside
group.Clear();

// React to added, removed or dead children
group.OnChildrenChanged += () => Debug.WriteLine(group.GetChildren().Count);
```

### Samples
```csharp
var group = new GameObjectGroup();

// --- Add ---
group.Add(new Enemy());
group.Add(new Coin());

// --- GetChildren ---
foreach (var child in group.GetChildren())
    Console.WriteLine(child);

// --- GetChildren<T> ---
foreach (var enemy in group.GetChildren<Enemy>())
    enemy.Alert();

// --- Remove a specific object ---
var boss = new Boss();
group.Add(boss);

// later…
group.Remove(boss);   // killed on next tick

// --- Clear everything ---
group.Clear();
```
