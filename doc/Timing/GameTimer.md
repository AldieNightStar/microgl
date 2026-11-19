# GameTimer

### Notes
* **Countdown and interval timer** implementing `IGameUpdatable`.
* **Supports single-shot and looping modes**, pause/resume via `IsPaused`, and completion callbacks via `OnComplete`.
* **Exposes normalized `Progress`** (`0.0f` to `1.0f`), `Remaining` time, and `IsFinished` state.

### Usage
* `new GameTimer(float duration, bool loop)`
  Creates a timer with target `duration` in seconds and whether it repeats (`loop`).
* `timer.Update(float dt)`
  Advances the timer by `dt` seconds; returns `true` and invokes `OnComplete` on the tick it finishes.
* `timer.Reset()`
  Resets `Elapsed` back to `0f` without changing `Duration`.
* `timer.Restart(float newDuration)`
  Sets a new `Duration` and resets `Elapsed` to `0f`.
* `timer.Dispose()`
  Pauses the timer, clears `OnComplete` subscribers, and resets `Elapsed`.

### Example
```csharp
var timer = new GameTimer(2.0f, loop: false);
timer.OnComplete += () => Console.WriteLine("Done!");

// Inside Update loop
timer.Update(dt);

// Reset or restart
timer.Reset();
timer.Restart(5.0f);
timer.Dispose();
```
