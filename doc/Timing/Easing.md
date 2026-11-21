# Easing

Curves that shape how a tween moves: start slow, end slow, overshoot, bounce. Pass one as the `ease` argument of a tween.

New to this? See [Tutorial 5: Tweens and animation](../Tutorials/05-Tweens.md).

### Notes
* **Static collection of standard interpolation curves** used by `Tween` and `TweenPlanner`.
* **Maps normalized progress `t`** (`0.0f` to `1.0f`) to eased output values.
* **Includes 31 easing functions** across `Linear`, `Sine`, `Quad`, `Cubic`, `Quart`, `Quint`, `Expo`, `Circ`, `Back`, `Elastic`, and `Bounce` (`EaseIn`, `EaseOut`, `EaseInOut`).

### Usage
* `Easing.Linear(float t)`
  Returns unmodified linear progress `t` (`0.0f` to `1.0f`).
* `Easing.EaseIn<Curve>(float t)` / `Easing.EaseOut<Curve>(float t)` / `Easing.EaseInOut<Curve>(float t)`
  Applies the chosen curve (`Sine`, `Quad`, `Cubic`, `Quart`, `Quint`, `Expo`, `Circ`, `Back`, `Elastic`, `Bounce`) to normalized time `t`.

### Choosing a curve
* `EaseOut...`: fast start, soft stop. Good for things that arrive, like a menu sliding in.
* `EaseIn...`: soft start, fast stop. Good for things that leave.
* `EaseInOut...`: soft at both ends. Good for moving between two places.
* `Back` overshoots a little, `Elastic` springs, `Bounce` bounces. `Sine` is gentle, `Expo` is sharp.

### Example
```csharp
var drop = new Tween(-100f, 200f, 0.8f, y => _logo.Position = new Vector2(360f, y), Easing.EaseOutBounce);

float halfway = Easing.EaseInOutCubic(0.5f);
```
