using System.Numerics;
using SDL3;

namespace microgl;

public sealed class GameFont : IDisposable
{
    private bool _disposed;

    internal nint _fontResource { get; private set; }

    public GameFont(string filename, float size)
    {
        TextEngine.Init();
        _fontResource = TTF.OpenFont(filename, size);
        if (_fontResource == IntPtr.Zero)
        {
            throw new Exception($"Font \"{filename}\" wasn't loaded successfully: {SDL.GetError()}");
        }
    }

    public static GameFont FromTTF(string filename, float size) => new(filename, size);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        TTF.CloseFont(_fontResource);
        _fontResource = IntPtr.Zero;
    }

    public Vector2 MeasureText(string text)
    {
        if (string.IsNullOrEmpty(text)) return Vector2.Zero;
        TTF.GetStringSize(_fontResource, text, 0, out int w, out int h);
        return new Vector2(w, h);
    }
}
