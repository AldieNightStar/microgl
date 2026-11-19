# TweenPlanner

### Notes
* **Fluent builder for sequential animations** that chains value tweens, delays, and instant callbacks into a single master `Tween`.
* **Supports `float` and `Vector2` interpolation**, timed waits, action triggers, and merging other `TweenPlanner` sequences via `Append`.
* **Compiles into a playable `Tween`** via `ToTween(bool repeat = false)` with seamless overflow propagation between steps and full loop reset support.

### Usage
```csharp
// Queue a float or Vector2 interpolation step over duration seconds
planner.Call(float from, float to, float duration, Action<float> callback, Func<float, float>? ease = null);
planner.Call(Vector2 from, Vector2 to, float duration, Action<Vector2> callback, Func<float, float>? ease = null);

// Queue a timed delay or an instant action callback
planner.Wait(float seconds);
planner.CallFunc(Action callback);

// Merge all steps from another TweenPlanner into this sequence
planner.Append(TweenPlanner other);

// Compile all queued steps into a single master Tween (optionally looping)
planner.ToTween(bool repeat = false);
```

### Example
```csharp
var intro = new TweenPlanner()
    .Call(0f, 1f, 0.5f, a => alpha = a, Easing.EaseOutQuad)
    .Wait(0.2f);

Tween master = new TweenPlanner()
    .Append(intro)
    .Call(Vector2.Zero, new Vector2(100f, 0f), 0.6f, p => pos = p, Easing.EaseInOutCubic)
    .CallFunc(() => Console.WriteLine("Done"))
    .ToTween(repeat: false);
```
