using System;
using System.Drawing;
using System.Numerics;

namespace microgl;

public class Label : UIElement
{
    public string Text
    {
        get => field;
        set
        {
            var val = value ?? string.Empty;
            if (field == val) return;
            field = val;
            RefreshTexture();
        }
    } = string.Empty;

    public GameFont Font
    {
        get => field;
        set
        {
            field = value;
            RefreshTexture();
        }
    }

    public Color TextColor
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            RefreshTexture();
        }
    } = Color.White;

    private DynamicTexture? _texture;

    public Label(GameFont font, Vector2 position, string text, Color? color = null, bool centerOrigin = false)
    {
        Position = position;
        CenterOrigin = centerOrigin;

        if (color.HasValue) TextColor = color.Value;
        Font = font;
        Text = text;
    }

    private void RefreshTexture()
    {
        Size = string.IsNullOrEmpty(Text) ? Vector2.Zero : Font.MeasureText(Text);

        ushort w = (ushort)Math.Ceiling(Size.X);
        ushort h = (ushort)Math.Ceiling(Size.Y);

        if (w == 0 || h == 0)
        {
            _texture?.Dispose();
            _texture = null;
            return;
        }

        if (_texture == null)
        {
            _texture = new DynamicTexture(w, h, DrawToTexture);
        }
        else if (_texture.Width != w || _texture.Height != h)
        {
            _texture.Resize(w, h);
        }
        else
        {
            _texture.UpdateTexture();
        }
    }

    private void DrawToTexture(Drawer d)
    {
        d.Font = Font;
        d.FontColor = TextColor;
        d.DrawText(Text, Vector2.Zero);
    }

    protected override void OnInit(Drawer d) { }

    protected override void OnUpdate(Drawer d) { }

    protected override void OnDraw(Drawer d)
    {
        if (_texture == null) return;

        d.DrawTexture(_texture, new Rectangle(0, 0, _texture.Width, _texture.Height));
    }

    public override void Dispose()
    {
        _texture?.Dispose();
        _texture = null;
    }
}
