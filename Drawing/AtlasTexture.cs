using System.Drawing;
using System.Numerics;
using SDL3;

namespace microgl;

public readonly struct AtlasTexture : IDisposable
{
    internal Texture _texture { get; init; }
    internal Rectangle _source { get; init; }

    public AtlasTexture(Texture atlas, Rectangle source)
    {
        _texture = atlas;
        _source = source;
    }

    public void Dispose()
    {
        _texture.Dispose();
    }

    public static AtlasTexture[] FromPNG(Drawer d, string filename, Vector2 tileSize)
    {
        var texture = Texture.FromPNG(d, filename);
        return _fromTexture(texture, d, filename, tileSize).ToArray();
    }

    public static AtlasTexture[] FromBMP(Drawer d, string filename, Vector2 tileSize)
    {
        var texture = Texture.FromBMP(d, filename);
        return _fromTexture(texture, d, filename, tileSize).ToArray();
    }

    private static IEnumerable<AtlasTexture> _fromTexture(Texture texture, Drawer d, string filename, Vector2 tileSize)
    {
        SDL.GetTextureSize(texture._resource, out var texW, out var texH);
        var countX = (int) (texW / tileSize.X);
        var countY = (int) (texW / tileSize.Y);
        var rects = _getRectangles((int) tileSize.X, (int) tileSize.Y, countX, countY);
        foreach (var rect in rects)
        {
            yield return new AtlasTexture(texture, rect);
        }
    }

    private static IEnumerable<Rectangle> _getRectangles(int w, int h, int countX, int countY)
    {
        for (int y = 0; y < countY; y++)
        {
            for (int x = 0; x < countX; x++)
            {
                yield return new Rectangle(x * w, y * h, w, h);
            }
        }
    }
}