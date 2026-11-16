using SDL3;

namespace microgl;

public readonly struct GameFont : IDisposable
{
    internal readonly nint _fontResource {get; init;}

    public GameFont(string filename, float size)
    {
        TextEngine.Init();
        _fontResource = TTF.OpenFont(filename, size);
        if (_fontResource == IntPtr.Zero)
        {
            throw new Exception($"Font \"{filename}\" wasn't loaded successfully: {SDL.GetError()}");
        }
    }

    public static GameFont FromTTF(string filename, float size)
    {
        return new GameFont(filename, size);
    }

    public void Dispose()
    {
        TTF.CloseFont(_fontResource);
    }
}