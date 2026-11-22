using System.Drawing;
using System.Numerics;
using SDL3;

namespace microgl;

internal static class QuadUtil
{
    public static readonly int[] QUAD_INDICES = [0, 1, 2, 0, 2, 3];

    public static void FillQuad(
        SDL.Vertex[]  verts,
        Rectangle     rect,
        float u0, float v0,
        float u1, float v1,
        Transform     transform,
        SDL.FColor    color)
    {
        var p0 = transform.ToGlobal(new Vector2(rect.Left,  rect.Top));
        var p1 = transform.ToGlobal(new Vector2(rect.Right, rect.Top));
        var p2 = transform.ToGlobal(new Vector2(rect.Right, rect.Bottom));
        var p3 = transform.ToGlobal(new Vector2(rect.Left,  rect.Bottom));

        verts[0] = new SDL.Vertex { Position = new SDL.FPoint { X = p0.X, Y = p0.Y }, Color = color, TexCoord = new SDL.FPoint { X = u0, Y = v0 } };
        verts[1] = new SDL.Vertex { Position = new SDL.FPoint { X = p1.X, Y = p1.Y }, Color = color, TexCoord = new SDL.FPoint { X = u1, Y = v0 } };
        verts[2] = new SDL.Vertex { Position = new SDL.FPoint { X = p2.X, Y = p2.Y }, Color = color, TexCoord = new SDL.FPoint { X = u1, Y = v1 } };
        verts[3] = new SDL.Vertex { Position = new SDL.FPoint { X = p3.X, Y = p3.Y }, Color = color, TexCoord = new SDL.FPoint { X = u0, Y = v1 } };
    }

    public static Rectangle TransformedBounds(Transform transform, Vector2 size)
    {
        var p0 = transform.ToGlobal(Vector2.Zero);
        var p1 = transform.ToGlobal(new Vector2(size.X, 0f));
        var p2 = transform.ToGlobal(size);
        var p3 = transform.ToGlobal(new Vector2(0f, size.Y));

        var min = Vector2.Min(Vector2.Min(p0, p1), Vector2.Min(p2, p3));
        var max = Vector2.Max(Vector2.Max(p0, p1), Vector2.Max(p2, p3));

        return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
    }
}
