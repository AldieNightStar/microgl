# TweenCollection

Ready-made game-feel effects: shakes, jumps, squishes, pops and more. Pass the object to animate, call `.ToTween()`, and update the result every frame.

New to this? See [Tutorial 5: Tweens and animation](../Tutorials/05-Tweens.md).

### Example
```csharp
private ITween? _effect;

private void _onEnemyHit() => _effect = TweenCollection.DamageShake(_enemy).ToTween();

public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
{
    _world.Update(d);
    _effect?.Update(dt);
}
```

Loop an idle effect with `repeat: true`:
```csharp
_idle = TweenCollection.Breathe(_slime).ToTween(repeat: true);
```

### Notes
* **Every effect returns a `TweenPlanner`**, so you can chain more steps before `.ToTween()`, for example `TweenCollection.PopIn(_title).Wait(1f).Append(TweenCollection.Pulse(_title))`.
* **Bound to the object**: every effect starts with `Tween.For(obj)`, so it stops by itself once `obj` is no longer `IsAlive`.
* **Start values are read when you call the effect**: the object's current `Position`, `Scale`, `Rotation` or `BlendColor` is the rest pose it returns to. Create effects while the object is at rest, not halfway through another effect.
* **Color effects change `BlendColor`**, which tints textures, labels and buttons, not shapes drawn with `drawer.Color`.
* **Position effects and `UITable`**: a table sets its elements' positions every frame, so position effects are undone there. Scale, rotation and color effects work.
* **`deleteOnComplete: true`** calls `obj.Delete()` when an exit effect ends.

### Impact and damage
```csharp
TweenCollection.DamageShake(GameObject obj, float intensity = 8f, float duration = 0.35f);   // fast horizontal jitter
TweenCollection.DamageFlash(GameObject obj, float duration = 0.3f);                          // flashes red twice
TweenCollection.Knockback(GameObject obj, Vector2 impulse, float duration = 0.4f);           // pushed back, then recovers
```

### Squash, stretch and jelly
```csharp
TweenCollection.Squish(GameObject obj, float amount = 0.3f, float duration = 0.45f);                    // flattens, springs back
TweenCollection.RiseFromSqueezedJelly(GameObject obj, float startSquish = 0.55f, float duration = 0.85f); // rises from flat with a wobble
TweenCollection.StretchVertical(GameObject obj, float amount = 0.35f, float duration = 0.45f);          // tall and thin, settles back
TweenCollection.JellyWobble(GameObject obj, float intensity = 0.25f, float duration = 0.7f);            // rubbery two-axis jiggle
```

### Jumps and bumps
```csharp
TweenCollection.JumpInPlace(GameObject obj, float height = 45f, float duration = 0.55f);     // straight up and down
TweenCollection.CartoonJump(GameObject obj, float height = 55f, float duration = 0.75f);     // crouch, jump, squash on landing
TweenCollection.BumpUp(GameObject obj, float distance = 18f, float duration = 0.25f);         // quick hop, like hitting a block
TweenCollection.BumpDirection(GameObject obj, Vector2 offset, float duration = 0.25f);       // quick nudge in a direction
```

### Wiggles and rotations
```csharp
TweenCollection.Wiggle(GameObject obj, float degrees = 15f, float duration = 0.5f);          // left-right rotation
TweenCollection.ShakeNo(GameObject obj, float distance = 12f, float duration = 0.4f);        // horizontal "no" shake
TweenCollection.NodYes(GameObject obj, float distance = 10f, float duration = 0.4f);         // vertical "yes" nod
TweenCollection.Spin360(GameObject obj, bool clockwise = true, float duration = 0.75f);      // full turn
```

### Pulses, idle, enter and exit
```csharp
TweenCollection.Pulse(GameObject obj, float scaleMultiplier = 1.2f, float duration = 0.4f);  // one scale pulse
TweenCollection.Breathe(GameObject obj, float amount = 0.06f, float duration = 1.6f);        // slow idle grow and shrink
TweenCollection.HoverFloat(GameObject obj, float amplitude = 8f, float duration = 1.8f);     // floats up and down
TweenCollection.PopIn(GameObject obj, float duration = 0.5f);                                // grows from 0 with a bounce
TweenCollection.PopOut(GameObject obj, bool deleteOnComplete = false, float duration = 0.35f); // shrinks and fades out
```

### Combat
```csharp
TweenCollection.LungeAttack(GameObject obj, Vector2 targetOffset, float duration = 0.45f);   // wind up, dash, return
TweenCollection.RecoilKick(GameObject obj, Vector2 kickbackOffset, float duration = 0.3f);   // sharp kick back, eases home
TweenCollection.ParryBounce(GameObject obj, float duration = 0.35f);                         // freeze, bright flash, scale pop
TweenCollection.DizzyStagger(GameObject obj, float swayDistance = 14f, float tiltDegrees = 18f, float duration = 1.2f); // stunned sway
```

### Coins and rewards
```csharp
TweenCollection.CoinSpin(GameObject obj, float duration = 0.6f);                             // fake 3D spin
TweenCollection.MagnetPull(GameObject obj, Vector2 targetPos, float duration = 0.55f);       // tugs back, then flies to a point
TweenCollection.TreasurePop(GameObject obj, float height = 65f, float duration = 0.85f);     // jumps out spinning, lands with a bounce
TweenCollection.LevelUpBurst(GameObject obj, float peakMultiplier = 1.4f, float duration = 0.8f); // grows with a golden flash
```

### UI and attention
```csharp
TweenCollection.ButtonPress(GameObject obj, float pressScale = 0.86f, float duration = 0.25f); // squish in, spring out
TweenCollection.AttentionRing(GameObject obj, float degrees = 20f, float duration = 0.65f);    // rings like a bell
TweenCollection.RubberSnap(GameObject obj, float pullX = 24f, float duration = 0.5f);         // stretches wide, snaps back
TweenCollection.TypewriterPop(GameObject obj, float dropDistance = 14f, float duration = 0.3f); // drops in and fades in
```

### Dramatic entrances and exits
```csharp
TweenCollection.DropSlam(GameObject obj, float dropHeight = 160f, float duration = 0.65f);   // falls from above, slams, rebounds
TweenCollection.PortalSwirl(GameObject obj, bool deleteOnComplete = false, float duration = 0.85f); // spins, shrinks, turns violet
TweenCollection.GhostFadeRise(GameObject obj, float riseDistance = 55f, bool deleteOnComplete = false, float duration = 1.1f); // floats up and fades
TweenCollection.FlipReveal(GameObject obj, float duration = 0.45f);                          // unfolds like a card
```

### Environment and creatures
```csharp
TweenCollection.PendulumSwing(GameObject obj, float maxDegrees = 25f, float duration = 1.6f); // swings like a hanging sign
TweenCollection.SlimeCrawl(GameObject obj, float stepX = 35f, float duration = 0.7f);         // stretches and moves one step
TweenCollection.EarthquakeRumble(GameObject obj, float intensity = 10f, float duration = 0.8f); // fading X and Y shake
TweenCollection.HeartbeatDouble(GameObject obj, float beatScale = 1.22f, float duration = 0.9f); // lub-dub pulse and rest
```
