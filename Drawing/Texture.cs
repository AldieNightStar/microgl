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
        Surface.Dispose();
    }
}
