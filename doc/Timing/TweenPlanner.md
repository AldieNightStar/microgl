# TweenPlanner

Builds a chain of animation steps: move, wait, run code. Call `ToTween()` to get a tween you can play.

New to this? See [Tutorial 5: Tweens and animation](../Tutorials/05-Tweens.md).

### Notes
* **Fluent builder** that queues value tweens, waits, instant callbacks, and any other `ITween` into a `TweenSequence`.
* **Supports `float` and `Vector2` interpolation**, timed waits, action triggers, custom steps via `Add`, and merging other planners via `Append`.
* **Object targets via `For(GameObject)`.** The built sequence stops as soon as any target is no longer `IsAlive`. `Append` keeps the other planner's targets.
* **`ToTween(bool repeat = false)`** returns a `TweenSequence` holding the queued steps. Build a new planner for each playback, since the steps are shared.

### Usage
```csharp
planner.For(GameObject target);

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

```csharp
ITween slide = Tween.For(enemy)
    .Call(enemy.Position, target, 0.4f, p => enemy.Position = p)
    .ToTween();

enemy.Delete();
slide.Update(d);
```
