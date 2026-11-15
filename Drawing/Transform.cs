using System.Numerics;

namespace microgl;

public readonly struct Transform
{
    public readonly Matrix3x2 Matrix;

    public static Transform Identity = new Transform(Matrix3x2.Identity);

    public Transform(Matrix3x2 mat)
    {
        Matrix = mat;
    }

    public Transform Move(Vector2 vec) => _of(Matrix3x2.CreateTranslation(vec));

    public Transform Scale(Vector2 scale) => _of(Matrix3x2.CreateScale(scale));

    public Transform Skew(Vector2 radVec) => _of(Matrix3x2.CreateSkew(radVec.X, radVec.Y));

    public Transform Rotate(float rad) => _of(Matrix3x2.CreateRotation(rad));

    public Transform RotateCentered(float rad, Vector2 size) => _of(Matrix3x2.CreateRotation(rad, size / 2f));

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
        return _of(Matrix3x2.CreateRotation(angle));
    }

    public Transform LookAtCentered(Vector2 fromCentered, Vector2 target, Vector2 size)
    {
        var dir = target - fromCentered;
        float angle = MathF.Atan2(dir.Y, dir.X);
        return _of(Matrix3x2.CreateRotation(angle, size / 2f));
    }

    private Transform _of(Matrix3x2 m) => new Transform(Matrix * m);
}