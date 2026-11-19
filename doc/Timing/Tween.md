# Tween

### Notes
* **Value interpolator** implementing `IGameUpdatable` that smoothly transitions a `float` from `From` to `To` over `Duration` seconds using an `Ease` curve.
* **Tracks playback state** through `Elapsed`, `Progress` (`0.0f` to `1.0f`), `CurrentValue`, `IsActive`, and `IsFinished`.
* **Supports looping and lifecycle events** via `Repeatable`, `IsPaused`, `OnUpdate`, and `OnComplete` (which passes any extra overflow time in seconds).
* **Acts as a fluent entry point** via static `Call`, `CallFunc`, and `Wait` helpers to build multi-step sequences with `TweenPlanner`.

### Usage
```csharp
// Create a tween from start to target value over duration seconds (defaults to Easing.Linear)
new Tween(float from, float to, float duration, Action<float>? onUpdate, Func<float, float>? ease = null);

// Start/end values, duration (seconds), easing curve, and tick/completion callbacks
tween.From; tween.To; tween.Duration;
tween.Ease; tween.OnUpdate; tween.OnComplete;

// Read-only progress tracking and playback flags
tween.Elapsed; tween.Progress; tween.CurrentValue;
tween.IsPaused; tween.Repeatable;
tween.IsActive; tween.IsFinished;

// Advance using Time.DeltaTime or explicit dt, reset state, or clean up
tween.Update();
tween.Update(float dt);
tween.Reset();
tween.Dispose();

// Fluent entry points for chaining multi-step sequences via TweenPlanner
Tween.Call(float from, float to, float duration, Action<float> callback, Func<float, float>? ease = null);
Tween.Call(Vector2 from, Vector2 to, float duration, Action<Vector2> callback, Func<float, float>? ease = null);
Tween.CallFunc(Action callback);
Tween.Wait(float seconds);
```

### Example
```csharp
// Basic tween with callbacks and looping
var tween = new Tween(0f, 100f, 1.0f, x => pos.X = x, Easing.EaseOutQuad);
tween.Repeatable = true;
tween.OnComplete += overflow => Console.WriteLine(\$"Done (overflow: {overflow})");

// Playback control & inspection
tween.Update(dt);
float p = tween.Progress;
bool active = tween.IsActive;
tween.IsPaused = true;
tween.Reset();
tween.Dispose();

// Chained sequence via TweenPlanner
var seq = Tween.Call(0f, 1f, 0.4f, a => alpha = a)
    .Wait(0.2f)
    .Call(Vector2.Zero, new Vector2(50f, 50f), 0.5f, v => pos = v, Easing.EaseInOutCubic)
    .CallFunc(() => Console.WriteLine("Sequence complete"))
    .ToTween();
```
