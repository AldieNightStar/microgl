namespace microgl;

using System;
using System.Numerics;
using SDL3;

public class Texture : IDisposable
{
    public Surface Surface { get; private set; }

    public Texture(Surface surface)
    {
        Surface = surface;
    }

    public static Texture FromPNG(string filename) => new(new Surface(SDL.LoadPNG(filename)));
    public static Texture FromBMP(string filename) => new(new Surface(SDL.LoadBMP(filename)));

    public Vector2 Size => Surface.Size;

    public void SetBlendMode(IRenderer renderer, SDL.BlendMode mode) => SDL.SetTextureBlendMode(renderer.GetNativeTexture(this), mode);

    internal nint ToSdlTexture(nint renderer)
    {
        nint sdlTexture = SDL.CreateTextureFromSurface(renderer, Surface.Pointer);
        if (sdlTexture == IntPtr.Zero) throw new Exception($"Failed to create SDL texture: {SDL.GetError()}");
        return sdlTexture;
    }

    public void Dispose()
    {
        if (Surface.Pointer == IntPtr.Zero) return;

        FreeingResourceBus<Texture>.Emit(this);
        Surface.Dispose();
    }

    public static Texture FromDrawing(ushort w, ushort h, Action<Drawer> act)
    {
        using SoftwareRenderer soft = new SoftwareRenderer(w, h);
        var drawer = new Drawer(soft)
        {
            ScreenSize = new Vector2(w, h),
        };
        act(drawer);
        var texture = soft.ProduceTexture(w, h);
        return texture;
    }
}
