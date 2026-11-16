using System.Numerics;

namespace microgl;

public readonly struct Transform
{
    public readonly Matrix3x2 Matrix;

    public static readonly Transform Identity = new(Matrix3x2.Identity);

    public Transform(Matrix3x2 mat)
    {
        Matrix = mat;
    }

    // --- Local Transformations (Relative to object's own rotation & scale) ---

    public Transform Move(Vector2 vec) => _local(Matrix3x2.CreateTranslation(vec));

    public Transform Scale(Vector2 scale) => _local(Matrix3x2.CreateScale(scale));

    public Transform Skew(Vector2 radVec) => _local(Matrix3x2.CreateSkew(radVec.X, radVec.Y));

    public Transform Rotate(float rad) => _local(Matrix3x2.CreateRotation(rad));

    public Transform RotateCentered(float rad, Vector2 size) => _local(Matrix3x2.CreateRotation(rad, size / 2f));

    // --- Global Transformations (Relative to parent / screen axes) ---

    public Transform GlobalMove(Vector2 vec) => _global(Matrix3x2.CreateTranslation(vec));

    public Transform GlobalScale(Vector2 scale) => _global(Matrix3x2.CreateScale(scale));

    public Transform GlobalSkew(Vector2 radVec) => _global(Matrix3x2.CreateSkew(radVec.X, radVec.Y));

    public Transform GlobalRotate(float rad) => _global(Matrix3x2.CreateRotation(rad));

    public Transform GlobalRotateAround(float rad, Vector2 worldPivot) => _global(Matrix3x2.CreateRotation(rad, worldPivot));

    public Vector2 Position => Matrix.Translation;

    public float Rotation => MathF.Atan2(Matrix.M12, Matrix.M11);

    public Transform WithPosition(Vector2 pos)
    {
        var m = Matrix;
        m.Translation = pos;
        return new Transform(m);
    }

    public Transform WithRotation(float rad)
    {
        // Preserve current position and scale, replace rotation
        Vector2 pos = Matrix.Translation;
        float scaleX = new Vector2(Matrix.M11, Matrix.M12).Length();
        float scaleY = new Vector2(Matrix.M21, Matrix.M22).Length();

        var m = Matrix3x2.CreateScale(scaleX, scaleY)
              * Matrix3x2.CreateRotation(rad)
              * Matrix3x2.CreateTranslation(pos);

        return new Transform(m);
    }

    public Transform WithScale(Vector2 scale)
    {
        // Preserve current position and rotation, replace scale
        Vector2 pos = Matrix.Translation;
        float rot = MathF.Atan2(Matrix.M12, Matrix.M11);

        var m = Matrix3x2.CreateScale(scale)
              * Matrix3x2.CreateRotation(rot)
              * Matrix3x2.CreateTranslation(pos);

        return new Transform(m);
    }

    // --- Hierarchy & Coordinate Conversions ---

    public static Transform operator *(Transform parent, Transform child) => new(child.Matrix * parent.Matrix);

    public Vector2 ToGlobal(Vector2 local) => Vector2.Transform(local, Matrix);

    public Vector2 ToLocal(Vector2 global)
    {
        Matrix3x2.Invert(Matrix, out var invertedMatrix);
        return Vector2.Transform(global, invertedMatrix);
    }

    public Transform LookAt(Vector2 from, Vector2 target)
    {
        var dir = target - from;
        float angle = MathF.Atan2(dir.Y, dir.X);
        return _local(Matrix3x2.CreateRotation(angle));
    }

    public Transform LookAtCentered(Vector2 fromCentered, Vector2 target, Vector2 size)
    {
        var dir = target - fromCentered;
        float angle = MathF.Atan2(dir.Y, dir.X);
        return _local(Matrix3x2.CreateRotation(angle, size / 2f));
    }

    // Applies 'm' first in local object space, then existing 'Matrix'
    private Transform _local(Matrix3x2 m) => new(m * Matrix);

    // Applies existing 'Matrix' first, then 'm' in parent/world space
    private Transform _global(Matrix3x2 m) => new(Matrix * m);
}
