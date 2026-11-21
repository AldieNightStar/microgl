# GameTimer

A countdown that fires `OnComplete` when it ends, once or over and over. Use it for cooldowns, spawn waves and delays.

New to this? See [Tutorial 5: Tweens and animation](../Tutorials/05-Tweens.md).

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
private const float SPAWN_INTERVAL = 2f;

private readonly GameTimer _spawnTimer = new(SPAWN_INTERVAL, loop: true);

public void Init(Drawer d)
{
    _spawnTimer.OnComplete += () => _world.Add(new Enemy());
}

public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
{
    _spawnTimer.Update(dt);
}
```

### Example: a cooldown
```csharp
private const float DASH_COOLDOWN = 1.5f;

private readonly GameTimer _dashCooldown = new(DASH_COOLDOWN, loop: false);
private bool _canDash = true;

public void Init(Drawer d)
{
    _dashCooldown.OnComplete += () => _canDash = true;
}

public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
{
    _dashCooldown.Update(dt);
}

private void _tryDash()
{
    if (!_canDash) return;
    _canDash = false;
    _player.Dash();
    _dashCooldown.Reset();
}
```

`Progress` goes from `0` to `1`, which is handy for a cooldown bar. `Restart(seconds)` sets a new duration and starts over, and `IsPaused` stops the countdown.
