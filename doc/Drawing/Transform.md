# Transform

### Notes
* **Immutable 2D affine transformation** `readonly struct` wrapping `System.Numerics.Matrix3x2`.
* **Separates local and global operations**: local methods (`Translate`, `Rotate`, `Scale`, `Skew`) apply in object space, while global methods (`GlobalTranslate`, `GlobalRotate`, `GlobalScale`, `GlobalSkew`) apply in parent/world space.
* **Supports hierarchy composition and coordinate conversion** via `parent * child`, `ToGlobal`, and `ToLocal`.

### Usage
```csharp
// Create from Matrix3x2 or use identity transform
new Transform(Matrix3x2 mat);
Transform.Identity;

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

// Decompose or replace Position, Rotation (radians), and ScaleFactor
t.Position; t.Rotation; t.ScaleFactor;
t.WithPosition(Vector2 pos);
t.WithRotation(float rad);
t.WithScale(Vector2 scale);

// Hierarchy multiplication, coordinate space conversion, and aim helpers
Transform combined = parent * child;
t.ToGlobal(Vector2 local);
t.ToLocal(Vector2 global);
t.LookAt(Vector2 from, Vector2 target);
t.LookAtCentered(Vector2 fromCentered, Vector2 target, Vector2 size);
```

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
