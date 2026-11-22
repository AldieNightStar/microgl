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

    public static Texture FromDrawing(IRenderer renderer, ushort w, ushort h, Action<Drawer> act)
    {
        var previousTarget = RenderTarget.GetTarget(renderer);
        using var target = RenderTarget.CreateTargetTexture(renderer, w, h);
        try
        {
            RenderTarget.SetTarget(renderer, target);
            _drawOnTarget(renderer, w, h, act);
            return new Texture(new Surface(SDL.RenderReadPixels(renderer.GetNativePointer(), null)));
        }
        finally
        {
            RenderTarget.SetTarget(renderer, previousTarget);
        }
    }

    private static void _drawOnTarget(IRenderer renderer, ushort w, ushort h, Action<Drawer> act)
    {
        nint native = renderer.GetNativePointer();
        SDL.GetRenderDrawColor(native, out byte r, out byte g, out byte b, out byte a);
        renderer.SetShader(null);
        SDL.SetRenderDrawColor(native, 0, 0, 0, 0);
        SDL.RenderClear(native);
        act(new Drawer(renderer) { ScreenSize = new Vector2(w, h) });
        SDL.SetRenderDrawColor(native, r, g, b, a);
    }
}
