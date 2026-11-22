using System.Drawing;

namespace microgl;

internal class TextureUtil
{
    public static (float u0, float v0, float u1, float v1) ComputeUVs(Texture texture, Rectangle src)
    {
        var size = texture.Size;
        return (
            src.X                / size.X,
            src.Y                / size.Y,
            (src.X + src.Width)  / size.X,
            (src.Y + src.Height) / size.Y
        );
    }
}
