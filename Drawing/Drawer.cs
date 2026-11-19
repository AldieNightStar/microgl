using System.Drawing;
using System.Numerics;
using SDL3;

namespace microgl;

public class Drawer : IDisposable
{
    private static readonly int[] QUAD_INDICES = [0, 1, 2, 0, 2, 3];
    private readonly SDL.Vertex[] _verts = new SDL.Vertex[4];

    public IRenderer Renderer { get; set; }
    public GameFont Font { get; set; }
    public Color FontColor { get; set; } = Color.White;
    public Transform Transform { get; set; } = Transform.Identity;
    public Vector2 ScreenSize {get; internal set;}

    public Drawer(IRenderer renderer)
    {
        Renderer = renderer;
    }

    public void Reset()
    {
        Clear();
        Transform = Transform.Identity;
        Font = default;
        FontColor = Color.White;
        BlendColor = Color.White;
    }

    public void Clear()
    {
        Color = Color.Black;
        SDL.RenderClear(Renderer.GetNativePointer());
        Color = Color.White;
    }

    public Color Color
    {
        get => field;
        set
        {
            field = value;
            SDL.SetRenderDrawColor(Renderer.GetNativePointer(), value.R, value.G, value.B, value.A);
        }
    } = Color.White;

    public Color BlendColor { get; set; } = Color.White;

    public void DrawTexture(Texture texture, Rectangle dstRect, Rectangle? srcRect = null)
    {
        float u0 = 0f, v0 = 0f, u1 = 1f, v1 = 1f;

        if (srcRect is Rectangle src)
        {
            var size = texture.Size;
            u0 = src.X / size.X;
            v0 = src.Y / size.Y;
            u1 = (src.X + src.Width) / size.X;
            v1 = (src.Y + src.Height) / size.Y;
        }

        _FillQuad(dstRect, u0, v0, u1, v1);

        // Using cache
        var nativeTexture = Renderer.GetNativeTexture(texture);

        // Apply blend color
        var blendFColor = _toFColor(BlendColor);
        _verts[0].Color = blendFColor;
        _verts[1].Color = blendFColor;
        _verts[2].Color = blendFColor;
        _verts[3].Color = blendFColor;

        // Drawing texture
        if (!SDL.RenderGeometry(Renderer.GetNativePointer(), nativeTexture, _verts, 4, QUAD_INDICES, 6))
        {
            throw new Exception($"Render failed for geometry: {SDL.GetError()}");
        }
    }

    public void DrawTexture(AtlasTexture texture, Rectangle rect) => DrawTexture(texture._texture, rect, texture._source);

    public void DrawTexture(DynamicTexture texture, Rectangle rect) => DrawTexture(texture.Texture, rect);

    public void DrawObject(GameObject obj) => obj.Draw(this, Transform);

    public void DrawObject(GameObject obj, Vector2 pos) => obj.Draw(this, Transform * Transform.Translate(pos));


    public void DrawRect(Rectangle rect)
    {
        _FillQuad(rect, 0f, 0f, 0f, 0f);
        SDL.RenderGeometry(Renderer.GetNativePointer(), 0, _verts, 4, QUAD_INDICES, 6);
    }

    public void DrawLine(Vector2 vec1, Vector2 vec2)
    {
        var p1 = Transform.ToGlobal(vec1);
        var p2 = Transform.ToGlobal(vec2);
        SDL.RenderLine(Renderer.GetNativePointer(), p1.X, p1.Y, p2.X, p2.Y);
    }

    public void DrawDebugText(string text, Vector2 vec)
    {
        var glob = Transform.ToGlobal(vec);
        SDL.RenderDebugText(Renderer.GetNativePointer(), glob.X, glob.Y, text);
    }

    public void DrawText(string text, Vector2 vec)
    {
        // TODO Cache textObject
        var vec2 = Transform.ToGlobal(vec);
        var textEnginePtr = TextEngine.Instance.GetEngineForRenderer(Renderer);

        var tempTextObject = TTF.CreateText(textEnginePtr, Font._fontResource, text, 0);
        if (tempTextObject == IntPtr.Zero) throw new Exception($"Text object wasn't create: {SDL.GetError()}");

        var color = FontColor;

        TTF.SetTextColor(tempTextObject, color.R, color.G, color.B, color.A);
        if (!TTF.DrawRendererText(tempTextObject, vec2.X, vec2.Y))
        {
            throw new Exception($"Can't draw text: {SDL.GetError()}");
        }
        TTF.DestroyText(tempTextObject);
        TextEngine.FreeEngineForRenderer(textEnginePtr, Renderer);
    }

    private void _FillQuad(Rectangle rect, float u0, float v0, float u1, float v1)
    {
        var p0 = Transform.ToGlobal(new Vector2(rect.Left, rect.Top));
        var p1 = Transform.ToGlobal(new Vector2(rect.Right, rect.Top));
        var p2 = Transform.ToGlobal(new Vector2(rect.Right, rect.Bottom));
        var p3 = Transform.ToGlobal(new Vector2(rect.Left, rect.Bottom));

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

    public void Dispose()
    {
        Renderer?.Dispose();
    }
}
