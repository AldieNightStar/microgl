using System.Numerics;

namespace microgl;

public readonly struct Transform
{
    public readonly Matrix3x2 Matrix;

    public static readonly Transform Identity = new(Matrix3x2.Identity);

    public static Transform Create(Vector2 position, float rotation, Vector2 scale, Vector2 skew = default)
    {
        var m = Matrix3x2.CreateScale(scale)
              * Matrix3x2.CreateSkew(skew.X, skew.Y)
              * Matrix3x2.CreateRotation(rotation)
              * Matrix3x2.CreateTranslation(position);
        return new Transform(m);
    }

    public static float DegreesToRadians(float degrees) => degrees * (MathF.PI / 180f);

    public static float RadiansToDegrees(float radians) => radians * (180f / MathF.PI);

    public Transform(Matrix3x2 mat)
    {
        Matrix = mat;
    }

    public Transform Translate(Vector2 vec) => _local(Matrix3x2.CreateTranslation(vec));

    public Transform Scale(Vector2 scale) => _local(Matrix3x2.CreateScale(scale));

    public Transform Skew(Vector2 radVec) => _local(Matrix3x2.CreateSkew(radVec.X, radVec.Y));

    public Transform Rotate(float rad) => _local(Matrix3x2.CreateRotation(rad));

    public Transform RotateCentered(float rad, Vector2 size) => _local(Matrix3x2.CreateRotation(rad, size / 2f));

    public Transform GlobalTranslate(Vector2 vec) => _global(Matrix3x2.CreateTranslation(vec));

    public Transform GlobalScale(Vector2 scale) => _global(Matrix3x2.CreateScale(scale));

    public Transform GlobalSkew(Vector2 radVec) => _global(Matrix3x2.CreateSkew(radVec.X, radVec.Y));

    public Transform GlobalRotate(float rad) => _global(Matrix3x2.CreateRotation(rad));

    public Transform GlobalRotateAround(float rad, Vector2 worldPivot) => _global(Matrix3x2.CreateRotation(rad, worldPivot));

    public Vector2 Position => Matrix.Translation;

    public float Rotation => MathF.Atan2(Matrix.M12, Matrix.M11);

    public Vector2 Right => new(MathF.Cos(Rotation), MathF.Sin(Rotation));

    public Vector2 Down => new(-MathF.Sin(Rotation), MathF.Cos(Rotation));

    public Vector2 ScaleFactor
    {
        get
        {
            float sx = new Vector2(Matrix.M11, Matrix.M12).Length();
            float sy = new Vector2(Matrix.M21, Matrix.M22).Length();
            float det = Matrix.M11 * Matrix.M22 - Matrix.M12 * Matrix.M21;
            return new Vector2(sx, det < 0f ? -sy : sy);
        }
    }

    public Transform WithPosition(Vector2 pos)
    {
        var m = Matrix;
        m.Translation = pos;
        return new Transform(m);
    }

    public Transform WithRotation(float rad)
    {
        Vector2 pos = Matrix.Translation;
        Vector2 scale = ScaleFactor;

        var m = Matrix3x2.CreateScale(scale)
              * Matrix3x2.CreateRotation(rad)
              * Matrix3x2.CreateTranslation(pos);

        return new Transform(m);
    }

    public Transform WithScale(Vector2 scale)
    {
        Vector2 pos = Matrix.Translation;
        float rot = Rotation;

        var m = Matrix3x2.CreateScale(scale)
              * Matrix3x2.CreateRotation(rot)
              * Matrix3x2.CreateTranslation(pos);

        return new Transform(m);
    }

    public Transform WithGlobalPosition(Transform parent, Vector2 globalPos)
        => WithPosition(parent.ToLocal(globalPos));

    public Transform WithGlobalRotation(Transform parent, float globalRot)
        => WithRotation(parent.LocalRotationFor(globalRot));

    public Transform WithGlobalScale(Transform parent, Vector2 globalScale)
        => WithScale(parent.LocalScaleFor(globalScale));

    public float LocalRotationFor(float globalRot) => globalRot - Rotation;

    public Vector2 ScaleChild(Vector2 childScale) => ScaleFactor * childScale;

    public Vector2 LocalScaleFor(Vector2 globalScale)
    {
        var ps = ScaleFactor;
        return new Vector2(
            ps.X != 0f ? globalScale.X / ps.X : 0f,
            ps.Y != 0f ? globalScale.Y / ps.Y : 0f);
    }

    public Vector2 RotateVector(Vector2 vec) => Vector2.Transform(vec, Matrix3x2.CreateRotation(Rotation));

    public static Transform operator *(Transform parent, Transform child) => new(child.Matrix * parent.Matrix);

    public Vector2 ToGlobal(Vector2 local) => Vector2.Transform(local, Matrix);

    public Vector2 ToLocal(Vector2 global) =>
        Matrix3x2.Invert(Matrix, out var invertedMatrix)
            ? Vector2.Transform(global, invertedMatrix)
            : Vector2.Zero;

    public bool TryGetAngleTo(Vector2 target, out float rad)
    {
        var dir = target - Position;
        rad = MathF.Atan2(dir.Y, dir.X);
        return dir.LengthSquared() != 0f;
    }

    public Transform LookAt(Vector2 target)
    {
        if (!TryGetAngleTo(target, out float rad)) return this;
        return WithRotation(rad);
    }

    public Transform LookAtCentered(Vector2 target, Vector2 size)
    {
        var center = ToGlobal(size / 2f);
        var dir = target - center;
        if (dir.LengthSquared() == 0f) return this;
        float targetAngle = MathF.Atan2(dir.Y, dir.X);
        return RotateCentered(targetAngle - Rotation, size);
    }

    private Transform _local(Matrix3x2 m) => new(m * Matrix);

    private Transform _global(Matrix3x2 m) => new(Matrix * m);
}
