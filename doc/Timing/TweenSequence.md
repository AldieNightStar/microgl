# TweenSequence

### Notes
* **`ITween` that holds a list of tweens** and plays them one after another. Built by `TweenPlanner.ToTween()`.
* **Carries lag over between steps.** Time left after a step finishes goes straight into the next step in the same update, so a long frame never skips or stalls a step.
* **Loops with `Repeat`.** Leftover time at the end of a cycle starts the next cycle. `OnComplete` fires at the end of every cycle.
* **Stops with dead targets.** When any target `GameObject` is no longer `IsAlive`, the sequence finishes on its next update without running more steps or `OnComplete`.
* **`Duration` and `Elapsed`** are the sums over all steps. Steps can be any `ITween`, including other sequences.

### Usage
```csharp
new TweenSequence(IEnumerable<ITween> steps, bool repeat = false, IEnumerable<GameObject>? targets = null);

sequence.Repeat;
sequence.Duration; sequence.Elapsed; sequence.Progress;
sequence.IsPaused; sequence.IsActive; sequence.IsFinished;
sequence.OnComplete;

float leftover = sequence.Update(float dt);
sequence.Update(Drawer d);
sequence.Reset();
sequence.Dispose();
```

### Example
```csharp
var fade = new Tween(0f, 1f, 0.3f, a => alpha = a);
var move = new Tween(0f, 200f, 0.5f, x => pos.X = x, Easing.EaseOutQuad);
var sequence = new TweenSequence(new ITween[] { fade, move }, repeat: true);

sequence.Update(0.5f);
```
