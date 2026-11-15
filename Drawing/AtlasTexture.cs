using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using SDL3;

namespace microgl;

public readonly struct AtlasTexture
{
    internal readonly Texture _texture { get; init; }
    internal readonly Rectangle _source { get; init; }

    public AtlasTexture(Texture atlas, Rectangle source)
    {
        _texture = atlas;
        _source = source;
    }

    public static AtlasTexture[] FromTexture(Texture t, Vector2 tileSize)
    {
        return _fromTexture(t, tileSize).ToArray();
    }

    private static IEnumerable<AtlasTexture> _fromTexture(Texture texture, Vector2 tileSize)
    {
        var size = texture.Size;
        var countX = (int) (size.X / tileSize.X);
        var countY = (int) (size.Y / tileSize.Y);
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