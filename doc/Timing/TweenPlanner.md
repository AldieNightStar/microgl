# TweenPlanner

### Notes
* **Fluent builder** that queues value tweens, waits, instant callbacks, and any other `ITween` into a `TweenSequence`.
* **Supports `float` and `Vector2` interpolation**, timed waits, action triggers, custom steps via `Add`, and merging other planners via `Append`.
* **`ToTween(bool repeat = false)`** returns a `TweenSequence` holding the queued steps. Build a new planner for each playback, since the steps are shared.

### Usage
```csharp
planner.Call(float from, float to, float duration, Action<float> callback, Func<float, float>? ease = null);
planner.Call(Vector2 from, Vector2 to, float duration, Action<Vector2> callback, Func<float, float>? ease = null);

planner.Wait(float seconds);
planner.CallFunc(Action callback);
planner.Add(ITween tween);

planner.Append(TweenPlanner other);

TweenSequence sequence = planner.ToTween(bool repeat = false);
```

### Example
```csharp
var intro = new TweenPlanner()
    .Call(0f, 1f, 0.5f, a => alpha = a, Easing.EaseOutQuad)
    .Wait(0.2f);

ITween master = new TweenPlanner()
    .Append(intro)
    .Call(Vector2.Zero, new Vector2(100f, 0f), 0.6f, p => pos = p, Easing.EaseInOutCubic)
    .CallFunc(() => Console.WriteLine("Done"))
    .ToTween();
```
