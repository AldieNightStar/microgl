# Tween

### Notes
* **Simple `ITween`** that moves one `float` from `From` to `To` over `Duration` seconds using an `Ease` curve.
* **`Update(float dt)` returns overflow time** past `Duration`, so a parent sequence can pass it to the next step.
* **Zero `Duration`** finishes on the first update, which is how instant callbacks work.
* **Fluent entry point** via static `Call`, `CallFunc`, and `Wait`, which start a `TweenPlanner`.
* **No built-in looping.** Use `Tween.Call(...).ToTween(repeat: true)` to loop.

### Usage
```csharp
new Tween(float from, float to, float duration, Action<float>? onUpdate, Func<float, float>? ease = null);

tween.From; tween.To; tween.Duration;
tween.Ease; tween.OnUpdate; tween.OnComplete;

tween.Elapsed; tween.Progress; tween.CurrentValue;
tween.IsPaused; tween.IsActive; tween.IsFinished;

float leftover = tween.Update(float dt);
tween.Update(Drawer d);
tween.Reset();
tween.Dispose();

Tween.Call(float from, float to, float duration, Action<float> callback, Func<float, float>? ease = null);
Tween.Call(Vector2 from, Vector2 to, float duration, Action<Vector2> callback, Func<float, float>? ease = null);
Tween.CallFunc(Action callback);
Tween.Wait(float seconds);
```

### Example
```csharp
var tween = new Tween(0f, 100f, 1.0f, x => pos.X = x, Easing.EaseOutQuad);
tween.OnComplete += () => Console.WriteLine("Done");
tween.Update(d);

var seq = Tween.Call(0f, 1f, 0.4f, a => alpha = a)
    .Wait(0.2f)
    .Call(Vector2.Zero, new Vector2(50f, 50f), 0.5f, v => pos = v, Easing.EaseInOutCubic)
    .CallFunc(() => Console.WriteLine("Sequence complete"))
    .ToTween();
```
