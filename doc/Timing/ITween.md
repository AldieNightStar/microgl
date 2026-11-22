# ITween

The common interface of all tweens. Store any tween as `ITween` and call `Update` on it every frame.

### Notes
* **Common interface for every tween**, implemented by the simple `Tween` and by `TweenSequence` (the result of `TweenPlanner.ToTween()`).
* **`Update(float dt)` returns the unused time.** A finished tween hands back the part of `dt` it did not need, so a lag spike carries over into the next step instead of being lost.
* **Extends `IGameUpdatable`**, so `Update(Drawer d)` advances it with `Time.DeltaTime`.

### Usage
```csharp
tween.Duration; tween.Elapsed; tween.Progress;
tween.IsPaused; tween.IsFinished; tween.IsActive;
tween.OnComplete += () => { };

float leftover = tween.Update(float dt);
tween.Update(Drawer d);
tween.Reset();
tween.Dispose();
```

### Example
```csharp
private ITween? _anim;

private void _celebrate()
{
    _anim = TweenCollection.Pulse(_button).ToTween();
    _anim.OnComplete += () => Console.WriteLine("Done");
}

public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
{
    _anim?.Update(dt);
}
```

### Example: run many tweens
```csharp
private readonly List<ITween> _tweens = new();

private void _updateTweens(float dt)
{
    for (int i = _tweens.Count - 1; i >= 0; i--)
        _tweens[i].Update(dt);
    _tweens.RemoveAll(t => t.IsFinished);
}
```
