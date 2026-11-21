using System.Drawing;

namespace microgl;

internal static class ShapeUtil
{
    private const float PIXEL_CENTER = 0.5f;

    public static List<Rectangle> RoundedStrips(Rectangle rect, int radius, List<Rectangle> strips)
    {
        strips.Clear();
        int r = Math.Clamp(radius, 0, Math.Min(rect.Width, rect.Height) / 2);

        for (int row = 0; row < r; row++)
        {
            int inset = CornerInset(r, row);
            int width = rect.Width - inset * 2;
            strips.Add(new Rectangle(rect.X + inset, rect.Y + row, width, 1));
            strips.Add(new Rectangle(rect.X + inset, rect.Bottom - row - 1, width, 1));
        }

        if (rect.Height > r * 2)
            strips.Add(new Rectangle(rect.X, rect.Y + r, rect.Width, rect.Height - r * 2));

        return strips;
    }

    public static (float u0, float v0, float u1, float v1) RelativeUVs(Rectangle inner, Rectangle outer) => (
        (float)(inner.Left - outer.Left) / outer.Width,
        (float)(inner.Top - outer.Top) / outer.Height,
        (float)(inner.Right - outer.Left) / outer.Width,
        (float)(inner.Bottom - outer.Top) / outer.Height);

    private static int CornerInset(int radius, int row)
    {
        float dy = radius - row - PIXEL_CENTER;
        return (int)MathF.Round(radius - MathF.Sqrt(radius * radius - dy * dy));
    }
}
