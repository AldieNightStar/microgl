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

    public void Dispose()
    {
        FreeingResourceBus.EmitFreedTexture(this);
        Surface.Dispose();
    }

    public static Texture FromDrawing(ushort w, ushort h, Action<Drawer> act)
    {
        using SoftwareRenderer soft = new SoftwareRenderer(w, h);
        using var drawer = new Drawer(soft)
        {
            ScreenSize = new Vector2(w, h),
        };
        act(drawer);
        var texture = soft.ProduceTexture(w, h);
        return texture;
    }
}
