using System.Drawing;
using System.Numerics;
using SDL3;

namespace microgl;

public class Drawer : IDisposable
{
    internal GPURenderer _gpuRenderer;

    private readonly SDL.Vertex[] _verts = new SDL.Vertex[4];
    private static readonly int[] _indices = [0, 1, 2, 0, 2, 3];
    private TextEngine _textEngine = new TextEngine();

    public Drawer(GPURenderer renderer)
    {
        _gpuRenderer = renderer;
        Renderer = null;
    }

    public void Reset()
    {
        Clear();
        Scale = Vector2.One;
        Transform = Transform.Identity;
        FontColor = Color.White;
    }

    public void Clear()
    {
        Color = Color.Black;
        SDL.RenderClear(Renderer.GetNativePointer());
        Color = Color.White;
    }

    public Transform Transform { get; set; } = Transform.Identity;

    public Color Color
    {
        get => field;
        set
        {
            field = value;
            SDL.SetRenderDrawColor(Renderer.GetNativePointer(), value.R, value.G, value.B, value.A);
        }
    } = Color.White;

    public Color FontColor { get; set; } = Color.White;

    public Vector2 Scale
    {
        get => field;
        set
        {
            field = value;
            SDL.SetRenderScale(Renderer.GetNativePointer(), value.X, value.Y);
        }
    }

    public GameFont Font { get; set; }

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
        var tempTexture = texture.GetTextureForRenderer(Renderer);
        if (!SDL.RenderGeometry(Renderer.GetNativePointer(), tempTexture, _verts, 4, _indices, 6))
        {
            throw new Exception($"Render failed for geometry: {SDL.GetError()}");
        }
        Texture.FreeTextureForRenderer(tempTexture, Renderer);
    }

    public void DrawObject(GameObject obj)
    {
        obj.Draw(this);
    }

    public void DrawTexture(AtlasTexture texture, Rectangle rect)
    {
        DrawTexture(texture._texture, rect, texture._source);
    }

    public void DrawRect(Rectangle rect)
    {
        _FillQuad(rect, 0f, 0f, 0f, 0f);
        SDL.RenderGeometry(Renderer.GetNativePointer(), 0, _verts, 4, _indices, 6);
    }

    public void DrawLine(Vector2 vec1, Vector2 vec2)
    {
        var p1 = Transform.ToGlobal(vec1);
        var p2 = Transform.ToGlobal(vec2);
        SDL.RenderLine(Renderer.GetNativePointer(), p1.X, p1.Y, p2.X, p2.Y);
    }

    public void DrawDebugText(string text, Vector2 vec)
    {
        var vec2 = Transform.ToGlobal(vec);
        SDL.RenderDebugText(Renderer.GetNativePointer(), vec2.X, vec2.Y, text);
    }

    public void DrawText(string text, Vector2 vec)
    {
        // TODO Cache textObject
        var vec2 = Transform.ToGlobal(vec);
        var textEnginePtr = _textEngine.GetEngineForRenderer(Renderer);
        var textObject = TTF.CreateText(textEnginePtr, Font._fontResource, text, 0);
        var color = FontColor;

        if (textObject == IntPtr.Zero) throw new Exception($"Text object wasn't create: {SDL.GetError()}");

        TTF.SetTextColor(textObject, color.R, color.G, color.B, color.A);
        if (!TTF.DrawRendererText(textObject, vec2.X, vec2.Y))
        {
            throw new Exception($"Can't draw text: {SDL.GetError()}");
        }
        TTF.DestroyText(textObject);
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

    public IRenderer Renderer
    {
        get
        {
            // Either return renderer or GPU Renderer as a fallback
            return field ?? _gpuRenderer;
        }
        set
        {
            // Do not close default renderers
            if (_isRendererNeedToClose(field)) field?.Dispose();
            field = value;
        }
    }

    internal SoftwareRenderer _softRenderer
    {
        get
        {
            if (field.SDLRenderer == IntPtr.Zero)
            {
                field = new SoftwareRenderer();
            }
            return field;
        }
    }

    public Texture GenerateTexture(ushort w, ushort h, Action<Drawer> action)
    {
        // Reuse software renderer
        Renderer = _softRenderer;
        Reset();

        action(this);
        var texture = _softRenderer.ProduceTexture(w, h);

        Renderer = null;
        return texture;
    }

    private bool _isRendererNeedToClose(IRenderer? r)
    {
        if (r == null) return false;

        var ptr = r?.GetNativePointer() ?? 0;
        if (ptr == _gpuRenderer.GetNativePointer() || ptr == _softRenderer.GetNativePointer()) return false;
        return true;
    }

    public void Dispose()
    {
        _textEngine.Dispose();
        Renderer?.Dispose();
    }
}
