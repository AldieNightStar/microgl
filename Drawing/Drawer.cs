using System.Drawing;
using System.Numerics;
using SDL3;

namespace microgl;

public class Drawer : IDisposable
{
    private static readonly int[] QUAD_INDICES = [0, 1, 2, 0, 2, 3];
    private readonly SDL.Vertex[] _verts = new SDL.Vertex[4];

    // == State ================================================================

    public IRenderer Renderer   { get; set; }
    public Vector2   ScreenSize { get; internal set; }

    /// <summary>Render transform (includes CenterPoint pivot). Used by all draw primitives.</summary>
    public Transform Transform      { get; set; } = Transform.Identity;

    /// <summary>Pure world transform of the current object (no pivot shift). Used by DrawObject as the child's parent.</summary>
    internal Transform WorldTransform { get; set; } = Transform.Identity;

    public Color  BlendColor { get; set; } = Color.White;
    public Color  FontColor  { get; set; } = Color.White;
    public GameFont Font     { get; set; }

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
        Transform      = Transform.Identity;
        WorldTransform = Transform.Identity;
        BlendColor     = Color.White;
        FontColor      = Color.White;
        Font           = default;
        Shader         = null;
    }

    // == Object drawing =======================================================

    /// <summary>Draws obj as a child of the current object (inherits WorldTransform).</summary>
    public void DrawObject(GameObject obj) => obj.Draw(this, WorldTransform);

    /// <summary>Draws obj at a local offset relative to the current object.</summary>
    public void DrawObject(GameObject obj, Vector2 localPos) => obj.Draw(this, WorldTransform * Transform.Translate(localPos));

    // == Masked drawing =======================================================

    /// <summary>
    /// Returns a scope that redirects drawing into an off-screen target, clips it to
    /// the shape of <paramref name="mask"/>, then blits the result to
    /// <paramref name="dstRect"/> when disposed. Areas where nothing was drawn remain
    /// fully transparent — the background shows through unchanged.
    ///
    /// <code>
    /// using (drawer.BeginMask(circleMask, 128, 128, new Rectangle(20, 20, 128, 128)))
    /// {
    ///     drawer.DrawTexture(playerPhoto, new Rectangle(0, 0, 128, 128));
    /// }
    /// </code>
    /// </summary>
    public MaskScope BeginMask(Texture mask, int width, int height, Rectangle dstRect)
        => new MaskScope(this, mask, width, height, dstRect);

    // == Primitive drawing ====================================================

    public void DrawTexture(Texture texture, Rectangle dstRect, Rectangle? srcRect = null)
    {
        float u0 = 0f, v0 = 0f, u1 = 1f, v1 = 1f;

        if (srcRect is Rectangle src)
        {
            var size = texture.Size;
            u0 = src.X / size.X;
            v0 = src.Y / size.Y;
            u1 = (src.X + src.Width)  / size.X;
            v1 = (src.Y + src.Height) / size.Y;
        }

        _FillQuad(dstRect, u0, v0, u1, v1);

        var nativeTexture = Renderer.GetNativeTexture(texture);
        var blendF        = _toFColor(BlendColor);
        _verts[0].Color   = blendF;
        _verts[1].Color   = blendF;
        _verts[2].Color   = blendF;
        _verts[3].Color   = blendF;

        if (!SDL.RenderGeometry(Renderer.GetNativePointer(), nativeTexture, _verts, 4, QUAD_INDICES, 6))
            throw new Exception($"RenderGeometry failed: {SDL.GetError()}");
    }

    public void DrawTexture(AtlasTexture   texture, Rectangle rect) => DrawTexture(texture._texture, rect, texture._source);
    public void DrawTexture(DynamicTexture texture, Rectangle rect) => DrawTexture(texture.Texture,  rect);

    public void DrawRect(Rectangle rect)
    {
        _FillQuad(rect, 0f, 0f, 0f, 0f);
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
        var p             = Transform.ToGlobal(pos);
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

    // == Internals ============================================================

    private void _FillQuad(Rectangle rect, float u0, float v0, float u1, float v1)
    {
        var p0 = Transform.ToGlobal(new Vector2(rect.Left,  rect.Top));
        var p1 = Transform.ToGlobal(new Vector2(rect.Right, rect.Top));
        var p2 = Transform.ToGlobal(new Vector2(rect.Right, rect.Bottom));
        var p3 = Transform.ToGlobal(new Vector2(rect.Left,  rect.Bottom));
        var fc = _toFColor(Color);

        _verts[0] = new SDL.Vertex { Position = new SDL.FPoint { X = p0.X, Y = p0.Y }, Color = fc, TexCoord = new SDL.FPoint { X = u0, Y = v0 } };
        _verts[1] = new SDL.Vertex { Position = new SDL.FPoint { X = p1.X, Y = p1.Y }, Color = fc, TexCoord = new SDL.FPoint { X = u1, Y = v0 } };
        _verts[2] = new SDL.Vertex { Position = new SDL.FPoint { X = p2.X, Y = p2.Y }, Color = fc, TexCoord = new SDL.FPoint { X = u1, Y = v1 } };
        _verts[3] = new SDL.Vertex { Position = new SDL.FPoint { X = p3.X, Y = p3.Y }, Color = fc, TexCoord = new SDL.FPoint { X = u0, Y = v1 } };
    }

    private static SDL.FColor _toFColor(Color c) =>
        new() { R = c.R / 255f, G = c.G / 255f, B = c.B / 255f, A = c.A / 255f };

    public void Dispose() => Renderer?.Dispose();
}
