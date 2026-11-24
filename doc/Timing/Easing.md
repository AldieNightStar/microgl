# Easing

Curves that shape how a tween moves: start slow, end slow, overshoot, bounce. Pass one as the `ease` argument of a tween.

### Notes
* **Static curves** that map progress `t` from `0` to `1` to an eased value.
* **Families**: `Linear`, then `Sine`, `Quad`, `Cubic`, `Quart`, `Quint`, `Expo`, `Circ`, `Back`, `Elastic` and `Bounce`. Each family except `Linear` has `EaseIn`, `EaseOut` and `EaseInOut` versions.
* **Choosing**: `EaseOut` starts fast and stops softly, for things that arrive. `EaseIn` starts softly and stops fast, for things that leave. `EaseInOut` is soft at both ends, for moving between two places.
* **Overshoot**: `Back` overshoots a little, `Elastic` springs, `Bounce` bounces.

### Usage
```csharp
Easing.Linear(float t);

Easing.EaseInSine(float t);       Easing.EaseOutSine(float t);       Easing.EaseInOutSine(float t);
Easing.EaseInQuad(float t);       Easing.EaseOutQuad(float t);       Easing.EaseInOutQuad(float t);
Easing.EaseInCubic(float t);      Easing.EaseOutCubic(float t);      Easing.EaseInOutCubic(float t);
Easing.EaseInQuart(float t);      Easing.EaseOutQuart(float t);      Easing.EaseInOutQuart(float t);
Easing.EaseInQuint(float t);      Easing.EaseOutQuint(float t);      Easing.EaseInOutQuint(float t);
Easing.EaseInExpo(float t);       Easing.EaseOutExpo(float t);       Easing.EaseInOutExpo(float t);
Easing.EaseInCirc(float t);       Easing.EaseOutCirc(float t);       Easing.EaseInOutCirc(float t);
Easing.EaseInBack(float t);       Easing.EaseOutBack(float t);       Easing.EaseInOutBack(float t);
Easing.EaseInElastic(float t);    Easing.EaseOutElastic(float t);    Easing.EaseInOutElastic(float t);
Easing.EaseInBounce(float t);     Easing.EaseOutBounce(float t);     Easing.EaseInOutBounce(float t);
```

### Example
```csharp
var drop = new Tween(-100f, 200f, 0.8f, y => _logo.Position = new Vector2(360f, y), Easing.EaseOutBounce);

float halfway = Easing.EaseInOutCubic(0.5f);
```
