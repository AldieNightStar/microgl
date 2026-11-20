using System.Drawing;
using System.Numerics;

namespace microgl;

public class UITable : UIElement
{
    // Sentinel — inserted into _flat on NextLine(); never added to _group.
    private sealed class NextLineMarker : UIElement
    {
        public NextLineMarker() : base(Vector2.Zero, Vector2.Zero) { }
        protected override void OnInit(Drawer d) { }
        protected override void OnUpdate(Drawer d) { }
        protected override void OnDraw(Drawer d) { }
        public override void Dispose() { }
    }

    // Flat ordered list of real elements + NextLineMarker sentinels.
    private readonly List<UIElement> _flat = new();
    // Owns lifecycle (Init / Update / Draw / Dispose) for real elements only.
    private readonly GameObjectGroup _group = new();
    // Cached layout bounds per element, keyed by reference.
    private readonly Dictionary<UIElement, Rectangle> _bounds = new();

    private Drawer _drawer;

    public Texture? BackgroundTexture { get; set; }
    public Color BackgroundColor { get; set; } = Color.Transparent;

    public float Spacing { get; set; } = 6f;
    public Vector2 Padding { get; set; } = Vector2.Zero;

    public UITable(Vector2 position, bool centerOrigin = false)
        : base(position, Vector2.Zero, centerOrigin)
    {
    }

    public UITable Add(Drawer d, UIElement element)
    {
        _flat.Add(element);
        _group.Add(element);
        Layout();

        if (_drawer != null)
            element.Init(_drawer, d.WorldTransform);

        return this;
    }

    public UITable NextLine()
    {
        // Avoid inserting two markers in a row.
        if (_flat.Count > 0 && _flat[^1] is not NextLineMarker)
            _flat.Add(new NextLineMarker());

        return this;
    }

    public void Layout()
    {
        float cursorY = Padding.Y;
        float maxRowWidth = 0f;

        // Collect one row at a time from the flat list.
        var row = new List<UIElement>();

        void FlushRow()
        {
            if (row.Count == 0) return;

            float rowHeight = 0f;
            foreach (var el in row)
                if (el.Size.Y > rowHeight) rowHeight = el.Size.Y;

            float cursorX = Padding.X;
            for (int i = 0; i < row.Count; i++)
            {
                var el = row[i];
                float y = cursorY + (rowHeight - el.Size.Y) / 2f;
                var bounds = new Rectangle((int)cursorX, (int)y, (int)el.Size.X, (int)el.Size.Y);
                _bounds[el] = bounds;
                el.Bounds = bounds;

                cursorX += el.Size.X + (i < row.Count - 1 ? Spacing : 0f);
            }

            if (cursorX > maxRowWidth) maxRowWidth = cursorX;
            cursorY += rowHeight + Spacing;
            row.Clear();
        }

        foreach (var item in _flat)
        {
            if (item is NextLineMarker) FlushRow();
            else row.Add(item);
        }
        FlushRow(); // last row

        if (cursorY > Padding.Y) cursorY -= Spacing;

        var contentSize = new Vector2(maxRowWidth + Padding.X, cursorY + Padding.Y);
        MaxSize = contentSize;
        Size = contentSize;
    }

    protected override void OnInit(Drawer d)
    {
        _drawer = d;
        Layout();
        _group.Init(d, d.WorldTransform);
    }

    protected override void OnUpdate(Drawer d)
    {
        // Sync positions from cached layout bounds, then delegate update.
        foreach (var el in _group.GetChildren<UIElement>())
        {
            if (_bounds.TryGetValue(el, out var b))
            {
                var expectedPos = new Vector2(b.X, b.Y) + el.CenterPoint;
                if (el.Position != expectedPos)
                    el.Position = expectedPos;
            }
        }

        _group.Update(d, d.WorldTransform);
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

        _group.Draw(d, d.WorldTransform);
    }

    public override void Dispose()
    {
        _group.Dispose();
        _flat.Clear();
        _bounds.Clear();
    }
}