using System.Drawing;
using SDL3;

namespace microgl;

internal static class ColorUtil
{
    public static SDL.FColor ToSDLColor(Color c) =>
        new() { R = c.R / 255f, G = c.G / 255f, B = c.B / 255f, A = c.A / 255f };
}
