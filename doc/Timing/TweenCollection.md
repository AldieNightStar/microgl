# TweenCollection

### Notes
* **Static library of preset `TweenPlanner` animations** for `GameObject` instances.
* **Covers game-feel effects** including damage reactions, squash-and-stretch, jumps, and bumps.
* **Returns `TweenPlanner` sequences** ready to chain or convert via `.ToTween()`.

### Usage
```csharp
// Impact: horizontal shake, red BlendColor flash, or directional pushback and return
TweenCollection.DamageShake(GameObject obj, float intensity = 8f, float duration = 0.35f);
TweenCollection.DamageFlash(GameObject obj, float duration = 0.3f);
TweenCollection.Knockback(GameObject obj, Vector2 impulse, float duration = 0.4f);

// Scale: squash, rise from squish, vertical stretch, or elastic jelly wobble
TweenCollection.Squish(GameObject obj, float amount = 0.3f, float duration = 0.45f);
TweenCollection.RiseFromSqueezedJelly(GameObject obj, float startSquish = 0.55f, float duration = 0.85f);
TweenCollection.StretchVertical(GameObject obj, float amount = 0.35f, float duration = 0.45f);
TweenCollection.JellyWobble(GameObject obj, float intensity = 0.25f, float duration = 0.7f);

// Movement: vertical hop, anticipation jump, upward bounce, or directional nudge
TweenCollection.JumpInPlace(GameObject obj, float height = 45f, float duration = 0.55f);
TweenCollection.CartoonJump(GameObject obj, float height = 55f, float duration = 0.75f);
TweenCollection.BumpUp(GameObject obj, float distance = 18f, float duration = 0.25f);
TweenCollection.BumpDirection(GameObject obj, Vector2 offset, float duration = 0.25f);
```

### Example
```csharp
Tween shake = TweenCollection.DamageShake(enemy).ToTween();
Tween wobble = TweenCollection.JellyWobble(slime).ToTween();
Tween jump = TweenCollection.CartoonJump(player, height: 60f).ToTween();
```
