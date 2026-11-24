# Transform

Position, rotation and scale in one value. Every `GameObject` has one. You need it directly only for coordinate math.

### Notes
* **Immutable 2D affine transformation** `readonly struct` wrapping `System.Numerics.Matrix3x2`.
* **Separates local and global operations**: local methods (`Translate`, `Rotate`, `Scale`, `Skew`) apply in object space, while global methods (`GlobalTranslate`, `GlobalRotate`, `GlobalScale`, `GlobalSkew`) apply in parent/world space.
* **Supports hierarchy composition and coordinate conversion** via `parent * child`, `ToGlobal`, and `ToLocal`.
* **Skew is dropped by some `With*` methods.** `WithPosition` keeps the full matrix. `WithRotation`, `WithScale`, `WithGlobalRotation`, `WithGlobalScale` and `LookAt` rebuild the transform from `Position`, `Rotation` and `ScaleFactor`, which cannot represent skew, so any skew is removed.
* **`ScaleFactor` is a decomposition**, so it does not describe skew.
* **`ToLocal` returns `Vector2.Zero`** when the matrix cannot be inverted, for example with a zero scale.

### Usage
```csharp
// Create from Matrix3x2, from position/rotation/scale/skew, or use identity
new Transform(Matrix3x2 mat);
Transform.Create(Vector2 position, float rotation, Vector2 scale, Vector2 skew = default);
Transform.Identity;

// Degree and radian helpers
Transform.DegreesToRadians(float degrees);
Transform.RadiansToDegrees(float radians);

// Local space transformations (relative to current rotation and scale)
t.Translate(Vector2 vec);
t.Scale(Vector2 scale);
t.Skew(Vector2 radVec);
t.Rotate(float rad);
t.RotateCentered(float rad, Vector2 size);

// Global space transformations (relative to parent/world axes)
t.GlobalTranslate(Vector2 vec);
t.GlobalScale(Vector2 scale);
t.GlobalSkew(Vector2 radVec);
t.GlobalRotate(float rad);
t.GlobalRotateAround(float rad, Vector2 worldPivot);

// Read Position, Rotation (radians), ScaleFactor, and the axes Right and Down
t.Position; t.Rotation; t.ScaleFactor; t.Right; t.Down;

// Replace Position, Rotation (radians), or Scale
t.WithPosition(Vector2 pos);
t.WithRotation(float rad);
t.WithScale(Vector2 scale);

// Set a value given in global (world) space, converted into the parent's space
t.WithGlobalPosition(Transform parent, Vector2 globalPos);
t.WithGlobalRotation(Transform parent, float globalRot);
t.WithGlobalScale(Transform parent, Vector2 globalScale);
t.LocalRotationFor(float globalRot);
t.LocalScaleFor(Vector2 globalScale);
t.ScaleChild(Vector2 childScale);

// Hierarchy multiplication, coordinate space conversion, and aim helpers
Transform combined = parent * child;
t.ToGlobal(Vector2 local);
t.ToLocal(Vector2 global);
t.RotateVector(Vector2 vec);
t.TryGetAngleTo(Vector2 target, out float rad);
t.LookAt(Vector2 target);
t.LookAtCentered(Vector2 target, Vector2 size);
```

`ScaleChild` multiplies the parent's scale factor by a child's scale. `RotateVector` rotates a vector by the transform's rotation only, ignoring scale and skew.

### Example
```csharp
var t = Transform.Identity
    .Translate(new Vector2(100f, 50f))
    .Rotate(0.5f)
    .Scale(new Vector2(2f, 2f));

Vector2 worldPos = t.ToGlobal(new Vector2(10f, 0f));
Vector2 localPos = t.ToLocal(worldPos);
t = t.WithPosition(new Vector2(200f, 150f));
```
