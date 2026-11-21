# ITween

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
ITween anim = TweenCollection.Pulse(button).ToTween();
anim.OnComplete += () => Console.WriteLine("Done");
anim.Update(d);
```
