using System.Drawing;
using System.Numerics;

namespace microgl;

public class UITable : UIElement
{
    private const float DEFAULT_SPACING = 6f;

    private readonly List<float> _rowHeights = new();
    private readonly List<float> _rowWidths = new();
    private readonly List<float> _columnWidths = new();
    private readonly List<GameObject> _children = new();
    private readonly List<(UIElement Element, int Row, int Column)> _cells = new();
    private Vector2 _contentSize;

    public GameObjectGroup Group { get; }

    public Insets Padding { get; set; }
    public Vector2 Spacing { get; set; } = new(DEFAULT_SPACING, DEFAULT_SPACING);
    public TextAlign HorizontalAlign { get; set; } = TextAlign.Center;
    public VerticalAlign VerticalAlign { get; set; } = VerticalAlign.Middle;
    public bool UniformColumns { get; set; } = true;
    public int Columns { get; set; }
    public bool AutoLayout { get; set; } = true;

    public Texture? BackgroundTexture { get; set; }
    public Color BackgroundColor { get; set; } = Color.Transparent;
    public int CornerRadius { get; set; }

    public int RowCount => _rowHeights.Count;
    public int ColumnCount => _columnWidths.Count;

    public UITable(Rectangle bounds, GameObjectGroup? group = null, bool centerOrigin = false)
        : base(bounds, centerOrigin)
    {
        Group = group ?? new GameObjectGroup();
        MaxSize = new Vector2(float.MaxValue, float.MaxValue);
        Group.OnChildrenChanged += OnGroupChanged;
    }

    protected virtual void OnGroupChanged() => Layout();

    public UITable Add(UIElement element)
    {
        Group.Add(element);
        return this;
    }

    public UITable AddRow(params UIElement[] elements)
    {
        foreach (var element in elements)
            Group.Add(element);
        return NextRow();
    }

    public UITable NextRow()
    {
        Group.Add(new NextRowMarker());
        return this;
    }

    public void Layout()
    {
        _collectCells();
        _measure();
        _placeCells();
        Size = _contentSize + Padding.Total;
    }

    private void _collectCells()
    {
        _cells.Clear();
        Group.CollectChildren(_children);
        int row = -1;
        int column = 0;

        foreach (var child in _children)
        {
            if (!child.IsAlive || !child.IsActive) continue;
            if (child is NextRowMarker || column == Columns) column = 0;
            if (child is not UIElement element) continue;
            if (column == 0) row++;
            _cells.Add((element, row, column++));
        }
    }

    private void _measure()
    {
        _rowHeights.Clear();
        _rowWidths.Clear();
        _columnWidths.Clear();

        var cellLimit = _cellLimit();

        foreach (var (element, row, column) in _cells)
        {
            element.AvailableSize = cellLimit;
            LayoutUtil.GrowAt(_rowHeights, row, element.Size.Y);
            LayoutUtil.GrowAt(_columnWidths, column, element.Size.X);
            LayoutUtil.GrowAt(_rowWidths, row, 0f);
            _rowWidths[row] += element.Size.X + (column > 0 ? Spacing.X : 0f);
        }

        float width = UniformColumns ? LayoutUtil.SpanLength(_columnWidths, Spacing.X) : (_rowWidths.Count == 0 ? 0f : _rowWidths.Max());
        _contentSize = new Vector2(width, LayoutUtil.SpanLength(_rowHeights, Spacing.Y));
    }

    private Vector2 _cellLimit()
    {
        var content = Vector2.Max(MaxSize - Padding.Total, Vector2.Zero);
        if (Columns <= 0) return content;
        return new Vector2(LayoutUtil.ShareLength(content.X, Columns, Spacing.X), content.Y);
    }

    private void _placeCells()
    {
        float x = 0f;
        float y = Padding.Top;

        foreach (var (element, row, column) in _cells)
        {
            if (column == 0)
            {
                x = _rowStart(row);
                if (row > 0) y += _rowHeights[row - 1] + Spacing.Y;
            }

            float cellWidth = UniformColumns ? _columnWidths[column] : element.Size.X;
            var cell = new RectangleF(x, y, cellWidth, _rowHeights[row]);
            element.Position = AlignUtil.Place(element.Size, cell, HorizontalAlign, VerticalAlign) + element.CenterPoint;
            x += cellWidth + Spacing.X;
        }
    }

    private float _rowStart(int row)
    {
        if (UniformColumns) return Padding.Left;
        return Padding.Left + AlignUtil.Offset(_contentSize.X - _rowWidths[row], HorizontalAlign);
    }

    protected override void OnInit(Drawer d)
    {
        Layout();
        Group.Init(d, d.Transform);
    }

    protected override void OnUpdate(Drawer d)
    {
        Group.Update(d, d.Transform);
        if (AutoLayout) Layout();
    }

    protected override void OnDraw(Drawer d)
    {
        _drawBackground(d);
        Group.Draw(d, d.Transform);
    }

    private void _drawBackground(Drawer d)
    {
        if (Size.X <= 0f || Size.Y <= 0f) return;

        if (BackgroundTexture != null)
        {
            d.DrawRoundedTexture(BackgroundTexture, LocalBounds, CornerRadius);
            return;
        }

        if (BackgroundColor.A == 0) return;
        d.Color = BackgroundColor;
        d.DrawRoundedRect(LocalBounds, CornerRadius);
    }

    public override void Dispose()
    {
        Group.OnChildrenChanged -= OnGroupChanged;
        Group.Dispose();
        _rowHeights.Clear();
        _rowWidths.Clear();
        _columnWidths.Clear();
        _children.Clear();
        _cells.Clear();
    }
}
