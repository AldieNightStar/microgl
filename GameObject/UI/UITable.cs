using System.Drawing;
using System.Numerics;

namespace microgl;

public class UITable : UIElement
{
    private sealed class Entry(UIElement element)
    {
        public UIElement Element { get; } = element;
        public Rectangle Bounds { get; set; }
    }

    private readonly List<List<Entry>> _rows = new() { new List<Entry>() };
    private Drawer _drawer;

    public Texture? BackgroundTexture { get; set; }
    public Color BackgroundColor { get; set; } = Color.Transparent;

    public float Spacing { get; set; } = 6f;
    public Vector2 Padding { get; set; } = Vector2.Zero;

    public UITable(Vector2 position, bool centerOrigin = false)
        : base(position, Vector2.Zero, centerOrigin)
    {
    }

    public UITable Add(UIElement element)
    {
        _rows[^1].Add(new Entry(element));
        Layout();

        if (_drawer != null)
            element.Init(_drawer, GlobalTransform);

        return this;
    }

    public UITable NextLine()
    {
        if (_rows[^1].Count > 0)
            _rows.Add(new List<Entry>());

        return this;
    }

    public void Layout()
    {
        float cursorY = Padding.Y;
        float maxRowWidth = 0f;

        for (int r = 0; r < _rows.Count; r++)
        {
            var row = _rows[r];
            if (row.Count == 0) continue;

            float cursorX = Padding.X;
            float rowHeight = 0f;

            for (int i = 0; i < row.Count; i++)
            {
                float h = row[i].Element.Size.Y;
                if (h > rowHeight) rowHeight = h;
            }

            for (int i = 0; i < row.Count; i++)
            {
                var entry = row[i];
                var el = entry.Element;
                float y = cursorY + (rowHeight - el.Size.Y) / 2f;

                entry.Bounds = new Rectangle((int)cursorX, (int)y, (int)el.Size.X, (int)el.Size.Y);
                el.Bounds = entry.Bounds;

                cursorX += el.Size.X + (i < row.Count - 1 ? Spacing : 0f);
            }

            if (cursorX > maxRowWidth)
                maxRowWidth = cursorX;

            cursorY += rowHeight + Spacing;
        }

        if (cursorY > Padding.Y)
            cursorY -= Spacing;

        var contentSize = new Vector2(maxRowWidth + Padding.X, cursorY + Padding.Y);
        MaxSize = contentSize;
        Size = contentSize;
    }

    protected override void OnInit(Drawer d)
    {
        _drawer = d;
        Layout();

        for (int r = 0; r < _rows.Count; r++)
            for (int i = 0; i < _rows[r].Count; i++)
                _rows[r][i].Element.Init(d, GlobalTransform);
    }

    protected override void OnUpdate(Drawer d)
    {
        for (int r = 0; r < _rows.Count; r++)
        {
            var row = _rows[r];
            for (int i = 0; i < row.Count; i++)
            {
                var entry = row[i];
                var el = entry.Element;

                Vector2 expectedPos = new Vector2(entry.Bounds.X, entry.Bounds.Y) + el.CenterPoint;
                if (el.Position != expectedPos)
                    el.Position = expectedPos;

                el.Update(d, GlobalTransform);
            }
        }
    }

    protected override void OnDraw(Drawer d)
    {
        if (Size.X > 0 && Size.Y > 0)
        {
            if (BackgroundTexture != null)
            {
                d.BlendColor = BlendColor;
                d.DrawTexture(BackgroundTexture, LocalBounds);
            }
            else if (BackgroundColor.A > 0)
            {
                d.Color = BackgroundColor;
                d.DrawRect(LocalBounds);
            }
        }

        for (int r = 0; r < _rows.Count; r++)
            for (int i = 0; i < _rows[r].Count; i++)
                _rows[r][i].Element.Draw(d, GlobalTransform);
    }

    public override void Dispose()
    {
        for (int r = 0; r < _rows.Count; r++)
            for (int i = 0; i < _rows[r].Count; i++)
                _rows[r][i].Element.Dispose();

        _rows.Clear();
        _rows.Add(new List<Entry>());
    }
}
