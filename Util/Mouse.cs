using System.Numerics;

namespace microgl;

public class Mouse
{
    internal static Vector2 LastPosition;

    public static Vector2 Position => LastPosition;
}