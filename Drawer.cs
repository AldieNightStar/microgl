using System.Drawing;
using System.Numerics;
using SDL3;

namespace microgl;

public class Drawer
{
    private nint _renderer;

    public Drawer(nint renderer)
    {
        _renderer = renderer;
    }

    public Color Color
    {
        get => field;
        set
        {
            field = value;
            SDL.SetRenderDrawColor(_renderer, value.R, value.G, value.B, value.A);
        }
    }

    public void Draw(Texture texture, Rectangle srcRect, Rectangle dstRect)
    {
        SDL.RenderTexture(_renderer, texture.Resource, _toFrect(srcRect), _toFrect(dstRect));
    }

    public void DrawRect(Rectangle rect)
    {
        SDL.RenderFillRect(_renderer, _toFrect(rect));
    }

    public void DrawLine(Vector2 vec1, Vector2 vec2)
    {
        SDL.RenderLine(_renderer, vec1.X, vec1.Y, vec2.X, vec2.Y);
    }

    private SDL.FRect _toFrect(Rectangle rect)
    {
        return new SDL.FRect { X = rect.X, Y = rect.Y, W = rect.Width, H = rect.Height };
    }
}