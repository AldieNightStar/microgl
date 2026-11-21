# Tutorial 5: Tweens and animation

A tween changes a value smoothly over time, for example a position from A to B in half a second. In this tutorial you play single tweens, chain them, loop them, use ready-made effects, and run timers.

You will use: [Tween](../Timing/Tween.md), [TweenPlanner](../Timing/TweenPlanner.md), [TweenSequence](../Timing/TweenSequence.md), [TweenCollection](../Timing/TweenCollection.md), [Easing](../Timing/Easing.md), [GameTimer](../Timing/GameTimer.md).

### Step 1: The one rule

A tween only moves when you update it. Keep it in a field and call `Update` every frame:

```csharp
private ITween? _slide;

public void Init(Drawer d)
{
    _slide = new Tween(0f, 400f, 0.5f, x => _box.Position = new Vector2(x, 100f));
}

public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
{
    _slide?.Update(dt);
}
```

The callback `x => ...` receives the current value every frame, from `0` to `400`, over `0.5` seconds.

### Step 2: Make it feel good with easing

The last argument picks a curve. `EaseOut` curves start fast and slow down, which feels natural for things that arrive. `EaseOutBack` overshoots a bit, `EaseOutBounce` bounces.

```csharp
_slide = new Tween(0f, 400f, 0.5f, x => _box.Position = new Vector2(x, 100f), Easing.EaseOutBack);
```

See [Easing](../Timing/Easing.md) for all 31 curves.

### Step 3: Chain steps

`Tween.Call` starts a plan of steps that play one after another. `ToTween()` turns the plan into a tween you can update.

```csharp
var start = new Vector2(100f, 100f);
var end = new Vector2(500f, 100f);

_slide = Tween.Call(start, end, 0.5f, p => _box.Position = p, Easing.EaseOutQuad)
    .Wait(0.3f)
    .Call(end, start, 0.5f, p => _box.Position = p, Easing.EaseInOutCubic)
    .CallFunc(() => Console.WriteLine("Back home"))
    .ToTween();
```

* `Call(from, to, seconds, callback, ease)` animates a `float` or a `Vector2`.
* `Wait(seconds)` pauses.
* `CallFunc(action)` runs code once.

### Step 4: Loop

Pass `repeat: true` and the plan starts again when it ends. `OnComplete` fires at the end of every round.

```csharp
const float BLINK_TIME = 0.4f;

_blink = Tween.Call(255f, 60f, BLINK_TIME, a => _arrow.BlendColor = Color.FromArgb((int)a, Color.White))
    .Call(60f, 255f, BLINK_TIME, a => _arrow.BlendColor = Color.FromArgb((int)a, Color.White))
    .ToTween(repeat: true);
```

Build a new plan every time you want a new tween. Two tweens made from the same plan share their steps.

### Step 5: Ready-made effects

`TweenCollection` has about 40 game-feel effects: shakes, jumps, squishes, pops and more. Each takes the object to animate and returns a plan.

```csharp
_hit = TweenCollection.DamageShake(_enemy).ToTween();
_idle = TweenCollection.Breathe(_slime).ToTween(repeat: true);
_entrance = TweenCollection.PopIn(_title).ToTween();
```

Three things to know:

* **Effects stop when the object dies.** After `_enemy.Delete()` the tween finishes by itself on its next update.
* **Effects read the object's current position and scale when you create them**, and return to those values. Create them while the object is at rest.
* **Color effects change `BlendColor`**, which tints textures, labels and buttons. Shapes drawn with `d.Color` are not tinted.

Inside a `UITable` the table sets the position of its elements every frame, so position effects are undone. Scale, rotation and color effects work.

### Step 6: Play many tweens at once

A small list is enough. Update every tween, then drop the finished ones. Going backwards lets a tween start a new one from its `OnComplete`.

```csharp
private readonly List<ITween> _tweens = new();

private void _play(TweenPlanner plan) => _tweens.Add(plan.ToTween());

private void _updateTweens(float dt)
{
    for (int i = _tweens.Count - 1; i >= 0; i--)
        _tweens[i].Update(dt);
    _tweens.RemoveAll(t => t.IsFinished);
}
```

```csharp
_enemy.OnHit += () => _play(TweenCollection.DamageFlash(_enemy));
_coin.OnPicked += () => _play(TweenCollection.PopOut(_coin, deleteOnComplete: true));
```

`OnHit` and `OnPicked` stand for your own events.

### Step 7: Timers

For "do something every N seconds", a [GameTimer](../Timing/GameTimer.md) is simpler than a tween.

```csharp
private const float SPAWN_INTERVAL = 2f;

private readonly GameTimer _spawnTimer = new(SPAWN_INTERVAL, loop: true);

public void Init(Drawer d)
{
    _spawnTimer.OnComplete += () => _world.Add(new Coin(_randomPoint(d)));
}

public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
{
    _spawnTimer.Update(dt);
}
```

### Step 8: Frame animations

Sprite sheets are animated with [AnimatedTexture](../Drawing/AnimatedTexture.md), not tweens. See [Tutorial 2](02-GameObjects.md#step-3-an-animated-player).
