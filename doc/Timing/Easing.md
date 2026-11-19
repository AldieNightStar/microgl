# Easing

### Notes
* **Static collection of standard interpolation curves** used by `Tween` and `TweenPlanner`.
* **Maps normalized progress `t`** (`0.0f` to `1.0f`) to eased output values.
* **Includes 31 easing functions** across `Linear`, `Sine`, `Quad`, `Cubic`, `Quart`, `Quint`, `Expo`, `Circ`, `Back`, `Elastic`, and `Bounce` (`EaseIn`, `EaseOut`, `EaseInOut`).

### Usage
* `Easing.Linear(float t)`
  Returns unmodified linear progress `t` (`0.0f` to `1.0f`).
* `Easing.EaseIn<Curve>(float t)` / `Easing.EaseOut<Curve>(float t)` / `Easing.EaseInOut<Curve>(float t)`
  Applies the chosen curve (`Sine`, `Quad`, `Cubic`, `Quart`, `Quint`, `Expo`, `Circ`, `Back`, `Elastic`, `Bounce`) to normalized time `t`.

### Example
```csharp
float value = Easing.EaseOutBounce(0.5f);
```
