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

    public static AtlasTexture[] FromTexture(Texture t, Vector2 tileSize, int spacing = 0, int margin = 0)
    {
        var size = t.Size;
        var usableRegion = new Rectangle(
            margin,
            margin,
            (int)size.X - margin * 2,
            (int)size.Y - margin * 2
        );
        return new AtlasTexture(t, usableRegion).SliceGrid(tileSize, spacing);
    }

    public static AtlasTexture[] FromRegion(Texture t, Rectangle region, Vector2 tileSize, int spacing = 0)
    {
        var subAtlas = new AtlasTexture(t, region);
        return subAtlas.SliceGrid(tileSize, spacing);
    }

    public AtlasTexture[] SliceGrid(Vector2 tileSize, int spacing = 0)
    {
        int w = (int)tileSize.X;
        int h = (int)tileSize.Y;
        int cols = (_source.Width + spacing) / (w + spacing);
        int rows = (_source.Height + spacing) / (h + spacing);

        var result = new AtlasTexture[cols * rows];
        int index = 0;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                var rect = new Rectangle(
                    _source.X + x * (w + spacing),
                    _source.Y + y * (h + spacing),
                    w,
                    h
                );
                result[index++] = new AtlasTexture(_texture, rect);
            }
        }

        return result;
    }

    public Vector2 Size => new(_source.Width, _source.Height);
    public Rectangle Bounds => _source;

    public static implicit operator AtlasTexture(Texture texture)
    {
        var size = texture.Size;
        return new AtlasTexture(texture, new Rectangle(0, 0, (int)size.X, (int)size.Y));
    }

    public AtlasTexture SubRegion(Rectangle relativeRect)
    {
        var offsetRect = new Rectangle(
            _source.X + relativeRect.X,
            _source.Y + relativeRect.Y,
            relativeRect.Width,
            relativeRect.Height
        );
        return new AtlasTexture(_texture, offsetRect);
    }
}