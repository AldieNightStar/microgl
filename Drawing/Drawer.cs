using System.Drawing;
using System.Numerics;
using SDL3;

namespace microgl;

public class Drawer : IDisposable
{
    private static readonly int[] QUAD_INDICES = [0, 1, 2, 0, 2, 3];
    private readonly SDL.Vertex[] _verts = new SDL.Vertex[4];

    // == State ================================================================

    public IRenderer Renderer { get; set; }
    public Vector2 ScreenSize { get; internal set; }
    public Rectangle ScreenBounds => new(0, 0, (int)ScreenSize.X, (int)ScreenSize.Y);

    /// <summary>Render transform (includes CenterPoint pivot). Used by all draw primitives.</summary>
    public Transform Transform { get; set; } = Transform.Identity;

    /// <summary>Pure world transform of the current object (no pivot shift). Used by DrawObject as the child's parent.</summary>
    internal Transform WorldTransform { get; set; } = Transform.Identity;

    public Color BlendColor { get; set; } = Color.White;
    public Color FontColor { get; set; } = Color.White;
    public GameFont Font { get; set; }

    public Color Color
    {
        get => field;
        set
        {
            field = value;
            SDL.SetRenderDrawColor(Renderer.GetNativePointer(), value.R, value.G, value.B, value.A);
        }
    } = Color.White;

    public Shader? Shader
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            Renderer.SetShader(value);
        }
    }

    // == Construction =========================================================

    public Drawer(IRenderer renderer) => Renderer = renderer;

    // == Frame lifecycle ======================================================

    public void Clear()
    {
        Color = Color.Black;
        SDL.RenderClear(Renderer.GetNativePointer());
        Color = Color.White;
    }

    /// <summary>Resets all per-frame draw state. Call once at the start of each Draw pass.</summary>
    public void Reset()
    {
        Clear();
        Transform = Transform.Identity;
        WorldTransform = Transform.Identity;
        BlendColor = Color.White;
        FontColor = Color.White;
        Font = default;
        Shader = null;
    }

    // == Object drawing =======================================================

    public void DrawObject(GameObject obj) => obj.Draw(this, WorldTransform);

    public void DrawObject(GameObject obj, Vector2 localPos) => obj.Draw(this, WorldTransform * Transform.Translate(localPos));

    // == Masked drawing =======================================================

    public MaskScope BeginMask(Texture mask, int width, int height, Rectangle dstRect)
        => new MaskScope(this, mask, width, height, dstRect);

    // == Primitive drawing ====================================================

    public void DrawTexture(Texture texture, Rectangle dstRect, Rectangle? srcRect = null)
    {
        float u0 = 0f, v0 = 0f, u1 = 1f, v1 = 1f;

        if (srcRect is Rectangle src)
            (u0, v0, u1, v1) = TextureUtil.ComputeUVs(texture, src);

        QuadUtil.FillQuad(_verts, dstRect, u0, v0, u1, v1, Transform, ColorUtil.ToSDLColor(Color));

        var nativeTexture = Renderer.GetNativeTexture(texture);
        var blendF = ColorUtil.ToSDLColor(BlendColor);
        _verts[0].Color = blendF;
        _verts[1].Color = blendF;
        _verts[2].Color = blendF;
        _verts[3].Color = blendF;

        if (!SDL.RenderGeometry(Renderer.GetNativePointer(), nativeTexture, _verts, 4, QUAD_INDICES, 6))
            throw new Exception($"RenderGeometry failed: {SDL.GetError()}");
    }

    public void DrawTexture(AtlasTexture texture, Rectangle rect) => DrawTexture(texture._texture, rect, texture._source);
    public void DrawTexture(DynamicTexture texture, Rectangle rect) => DrawTexture(texture.Texture, rect);

    public void DrawRect(Rectangle rect)
    {
        QuadUtil.FillQuad(_verts, rect, 0f, 0f, 0f, 0f, Transform, ColorUtil.ToSDLColor(Color));
        SDL.RenderGeometry(Renderer.GetNativePointer(), 0, _verts, 4, QUAD_INDICES, 6);
    }

    public void DrawLine(Vector2 a, Vector2 b)
    {
        var p0 = Transform.ToGlobal(a);
        var p1 = Transform.ToGlobal(b);
        SDL.RenderLine(Renderer.GetNativePointer(), p0.X, p0.Y, p1.X, p1.Y);
    }

    public void DrawDebugText(string text, Vector2 pos)
    {
        var p = Transform.ToGlobal(pos);
        SDL.RenderDebugText(Renderer.GetNativePointer(), p.X, p.Y, text);
    }

    public void DrawText(string text, Vector2 pos)
    {
        var p = Transform.ToGlobal(pos);
        var textEnginePtr = TextEngine.Instance.GetEngineForRenderer(Renderer);

        var obj = TTF.CreateText(textEnginePtr, Font._fontResource, text, 0);
        if (obj == IntPtr.Zero)
            throw new Exception($"TTF.CreateText failed: {SDL.GetError()}");

        TTF.SetTextColor(obj, FontColor.R, FontColor.G, FontColor.B, FontColor.A);

        if (!TTF.DrawRendererText(obj, p.X, p.Y))
            throw new Exception($"TTF.DrawRendererText failed: {SDL.GetError()}");

        TTF.DestroyText(obj);
        TextEngine.FreeEngineForRenderer(textEnginePtr, Renderer);
    }

    public void Dispose() => Renderer?.Dispose();
}
