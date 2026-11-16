namespace microgl;

public class DynamicTexture : IDisposable
{
    public ushort Width { get; private set; }
    public ushort Height { get; private set; }
    private Action<Drawer> _draw;
    private Texture _texture;

    public Texture Texture
    {
        get
        {
            if (_texture == null) UpdateTexture();
            return _texture!;
        }
    }

    public DynamicTexture(ushort w, ushort h, Action<Drawer> drawing)
    {
        Width = w;
        Height = h;
        _draw = drawing;
    }

    public void UpdateTexture()
    {
        if (_texture != null) _texture.Dispose();
        _texture = Texture.FromDrawing(Width, Height, _draw);
    }

    public void Dispose()
    {
        if (_texture != null) {
            _texture.Dispose();
            _texture = null;
        }
    }

    public static implicit operator Texture(DynamicTexture t) => t.Texture;
}