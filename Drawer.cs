using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using SDL3;

namespace microgl;

public class Drawer
{
    internal readonly nint _renderer;
    private readonly SDL.Vertex[] _verts = new SDL.Vertex[4];
    private static readonly int[] _indices = [0, 1, 2, 0, 2, 3];
    private static readonly SDL.FColor _white = new() { R = 1f, G = 1f, B = 1f, A = 1f };

    public Drawer(nint renderer)
    {
        _renderer = renderer;
    }

    public Matrix4x4 Matrix { get; set; } = Matrix4x4.Identity;

    public Color Color
    {
        get => field;
        set
        {
            field = value;
            SDL.SetRenderDrawColor(_renderer, value.R, value.G, value.B, value.A);
        }
    } = Color.White;

    public void DrawTexture(Texture texture, Rectangle dstRect, Rectangle? srcRect = null)
    {
        float u0 = 0f, v0 = 0f, u1 = 1f, v1 = 1f;

        if (srcRect is Rectangle src)
        {
            SDL.GetTextureSize(texture.Resource, out float texW, out float texH);
            u0 = src.X / texW;
            v0 = src.Y / texH;
            u1 = (src.X + src.Width) / texW;
            v1 = (src.Y + src.Height) / texH;
        }

        _FillQuad(dstRect, u0, v0, u1, v1);
        if (!SDL.RenderGeometry(_renderer, texture.Resource, _verts, 4, _indices, 6))
        {
            Debug.WriteLine($"Render failed for geometry: {SDL.GetError()}");
        }
    }

    public void DrawRect(Rectangle rect)
    {
        _FillQuad(rect, 0f, 0f, 0f, 0f);
        SDL.RenderGeometry(_renderer, 0, _verts, 4, _indices, 6);
    }

    public void DrawLine(Vector2 vec1, Vector2 vec2)
    {
        var p1 = Vector2.Transform(vec1, Matrix);
        var p2 = Vector2.Transform(vec2, Matrix);
        SDL.RenderLine(_renderer, p1.X, p1.Y, p2.X, p2.Y);
    }

    public void Clear()
    {
        Color = Color.Black;
        SDL.RenderClear(_renderer);
        Color = Color.White;
    }

    private void _FillQuad(Rectangle rect, float u0, float v0, float u1, float v1)
    {
        var p0 = Vector2.Transform(new Vector2(rect.Left, rect.Top), Matrix);
        var p1 = Vector2.Transform(new Vector2(rect.Right, rect.Top), Matrix);
        var p2 = Vector2.Transform(new Vector2(rect.Right, rect.Bottom), Matrix);
        var p3 = Vector2.Transform(new Vector2(rect.Left, rect.Bottom), Matrix);

        var fcolor = _toFColor(Color);

        _verts[0] = new SDL.Vertex { Position = new SDL.FPoint { X = p0.X, Y = p0.Y }, Color = fcolor, TexCoord = new SDL.FPoint { X = u0, Y = v0 } };
        _verts[1] = new SDL.Vertex { Position = new SDL.FPoint { X = p1.X, Y = p1.Y }, Color = fcolor, TexCoord = new SDL.FPoint { X = u1, Y = v0 } };
        _verts[2] = new SDL.Vertex { Position = new SDL.FPoint { X = p2.X, Y = p2.Y }, Color = fcolor, TexCoord = new SDL.FPoint { X = u1, Y = v1 } };
        _verts[3] = new SDL.Vertex { Position = new SDL.FPoint { X = p3.X, Y = p3.Y }, Color = fcolor, TexCoord = new SDL.FPoint { X = u0, Y = v1 } };
    }

    private static SDL.FColor _toFColor(Color c) => new()
    {
        R = c.R / 255f,
        G = c.G / 255f,
        B = c.B / 255f,
        A = c.A / 255f,
    };
}
