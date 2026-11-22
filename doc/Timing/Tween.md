# Tween

Moves one number from a start value to an end value over time. It is also the starting point for chained animations with `Tween.Call(...)`.

### Notes
* **Simple `ITween`** that moves one `float` from `From` to `To` over `Duration` seconds using an `Ease` curve.
* **`Update(float dt)` returns overflow time** past `Duration`, so a parent sequence can pass it to the next step.
* **Zero `Duration`** finishes on the first update, which is how instant callbacks work.
* **Fluent entry point** via static `For`, `Call`, `CallFunc`, and `Wait`, which start a `TweenPlanner`. `For(obj)` stops the sequence once `obj` is dead.
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

Tween.For(GameObject target);
Tween.Call(float from, float to, float duration, Action<float> callback, Func<float, float>? ease = null);
Tween.Call(Vector2 from, Vector2 to, float duration, Action<Vector2> callback, Func<float, float>? ease = null);
Tween.CallFunc(Action callback);
Tween.Wait(float seconds);
```

### Example
A tween only moves when you update it, so keep it in a field and update it every frame.
```csharp
private Tween _fadeIn = null!;

public void Init(Drawer d)
{
    _fadeIn = new Tween(0f, 255f, 1.0f, a => _title.BlendColor = Color.FromArgb((int)a, Color.White), Easing.EaseOutQuad);
    _fadeIn.OnComplete += () => Console.WriteLine("Title visible");
}

public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
{
    _fadeIn.Update(dt);
}
```

### Example: a chain of steps
`Tween.Call`, `Wait`, `CallFunc` and `For` start a [TweenPlanner](TweenPlanner.md). `ToTween()` turns it into one tween.
```csharp
var seq = Tween.Call(0f, 1f, 0.4f, a => alpha = a)
    .Wait(0.2f)
    .Call(Vector2.Zero, new Vector2(50f, 50f), 0.5f, v => pos = v, Easing.EaseInOutCubic)
    .CallFunc(() => Console.WriteLine("Sequence complete"))
    .ToTween();
```
