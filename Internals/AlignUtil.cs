using System.Drawing;
using System.Numerics;

namespace microgl;

internal static class AlignUtil
{
    public static float Offset(float freeSpace, TextAlign align) => align switch
    {
        TextAlign.Center => freeSpace / 2f,
        TextAlign.Right => freeSpace,
        _ => 0f
    };

    public static float Offset(float freeSpace, VerticalAlign align) => align switch
    {
        VerticalAlign.Middle => freeSpace / 2f,
        VerticalAlign.Bottom => freeSpace,
        _ => 0f
    };

    public static Vector2 Place(Vector2 size, RectangleF cell, TextAlign horizontal, VerticalAlign vertical) => new(
        cell.X + Offset(cell.Width - size.X, horizontal),
        cell.Y + Offset(cell.Height - size.Y, vertical));
}
