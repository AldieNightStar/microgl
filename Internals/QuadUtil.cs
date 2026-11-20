using System.Drawing;
using System.Numerics;
using SDL3;

namespace microgl;

internal static class QuadUtil
{
    /// <summary>
    /// Fills <paramref name="verts"/> (must have length ≥ 4) with the four corners of
    /// <paramref name="rect"/> transformed through <paramref name="transform"/>,
    /// UV-mapped to [u0,v0]–[u1,v1], and tinted with <paramref name="color"/>.
    /// </summary>
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
}
