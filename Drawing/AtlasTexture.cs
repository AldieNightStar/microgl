using System.Drawing;
using System.Numerics;

namespace microgl;

public readonly struct AtlasTexture
{
    public Texture Texture { get; }
    public Rectangle Bounds { get; }

    public AtlasTexture(Texture atlas, Rectangle source)
    {
        Texture = atlas;
        Bounds = source;
    }

    public Vector2 Size => new(Bounds.Width, Bounds.Height);

    public static AtlasTexture[] FromTexture(Texture t, Vector2 tileSize, int spacing = 0, int margin = 0)
    {
        var size = t.Size;
        var usableRegion = new Rectangle(margin, margin, (int)size.X - margin * 2, (int)size.Y - margin * 2);
        return FromRegion(t, usableRegion, tileSize, spacing);
    }

    public static AtlasTexture[] FromRegion(Texture t, Rectangle region, Vector2 tileSize, int spacing = 0)
        => new AtlasTexture(t, region).SliceGrid(tileSize, spacing);

    public AtlasTexture[] SliceGrid(Vector2 tileSize, int spacing = 0)
    {
        int w = (int)tileSize.X;
        int h = (int)tileSize.Y;
        if (w <= 0 || h <= 0 || spacing < 0)
            throw new ArgumentOutOfRangeException(nameof(tileSize), "Tile size must be positive and spacing non-negative.");

        int cols = Math.Max(0, (Bounds.Width + spacing) / (w + spacing));
        int rows = Math.Max(0, (Bounds.Height + spacing) / (h + spacing));
        var result = new AtlasTexture[cols * rows];

        for (int i = 0; i < result.Length; i++)
            result[i] = SubRegion(new Rectangle(i % cols * (w + spacing), i / cols * (h + spacing), w, h));

        return result;
    }

    public AtlasTexture SubRegion(Rectangle relativeRect)
    {
        var rect = relativeRect with { X = Bounds.X + relativeRect.X, Y = Bounds.Y + relativeRect.Y };
        if (!Bounds.Contains(rect))
            throw new ArgumentOutOfRangeException(nameof(relativeRect), $"Region ({relativeRect}) exceeds atlas bounds ({Bounds}).");

        return new AtlasTexture(Texture, rect);
    }
}
