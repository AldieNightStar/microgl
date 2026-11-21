using System.Numerics;

namespace microgl;

public readonly record struct Insets(float Left, float Top, float Right, float Bottom)
{
    public static Insets All(float value) => new(value, value, value, value);

    public static Insets Symmetric(float horizontal, float vertical) => new(horizontal, vertical, horizontal, vertical);

    public float Horizontal => Left + Right;
    public float Vertical => Top + Bottom;
    public Vector2 TopLeft => new(Left, Top);
    public Vector2 Total => new(Horizontal, Vertical);

    public static implicit operator Insets(float value) => All(value);
    public static implicit operator Insets(Vector2 value) => Symmetric(value.X, value.Y);
}
