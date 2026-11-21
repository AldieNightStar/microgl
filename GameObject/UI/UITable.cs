using System.Drawing;
using System.Numerics;

namespace microgl;

public class UITable : UIElement
{
    private const float DEFAULT_SPACING = 6f;

    private readonly List<float> _rowHeights = new();
    private readonly List<float> _rowWidths = new();
    private readonly List<float> _columnWidths = new();
    private Vector2 _contentSize;

    public GameObjectGroup Group { get; }

    public Insets Padding { get; set; }
    public Vector2 Spacing { get; set; } = new(DEFAULT_SPACING, DEFAULT_SPACING);
    public TextAlign HorizontalAlign { get; set; } = TextAlign.Left;
    public VerticalAlign VerticalAlign { get; set; } = VerticalAlign.Middle;
    public bool UniformColumns { get; set; } = true;
    public int Columns { get; set; }
    public bool AutoLayout { get; set; } = true;

    public Texture? BackgroundTexture { get; set; }
    public Color BackgroundColor { get; set; } = Color.Transparent;
    public int CornerRadius { get; set; }

    public int RowCount => _rowHeights.Count;
    public int ColumnCount => _columnWidths.Count;

    public UITable(Vector2 position, GameObjectGroup? group = null, bool centerOrigin = false)
        : base(position, Vector2.Zero, centerOrigin)
    {
        Group = group ?? new GameObjectGroup();
        MaxSize = new Vector2(float.MaxValue, float.MaxValue);
    }

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
        Measure();
        PlaceCells();
        Size = _contentSize + Padding.Total;
    }

    private IEnumerable<(UIElement Element, int Row, int Column)> Cells()
    {
        int row = -1;
        int column = 0;

        foreach (var child in Group.GetChildren<GameObject>())
        {
            if (!child.IsAlive || !child.IsActive) continue;
            if (child is NextRowMarker || column == Columns) column = 0;
            if (child is not UIElement element) continue;
            if (column == 0) row++;
            yield return (element, row, column++);
        }
    }

    private void Measure()
    {
        _rowHeights.Clear();
        _rowWidths.Clear();
        _columnWidths.Clear();

        foreach (var (element, row, column) in Cells())
        {
            LayoutUtil.GrowAt(_rowHeights, row, element.Size.Y);
            LayoutUtil.GrowAt(_columnWidths, column, element.Size.X);
            LayoutUtil.GrowAt(_rowWidths, row, 0f);
            _rowWidths[row] += element.Size.X + (column > 0 ? Spacing.X : 0f);
        }

        float width = UniformColumns ? LayoutUtil.SpanLength(_columnWidths, Spacing.X) : _rowWidths.DefaultIfEmpty().Max();
        _contentSize = new Vector2(width, LayoutUtil.SpanLength(_rowHeights, Spacing.Y));
    }

    private void PlaceCells()
    {
        float x = 0f;
        float y = Padding.Top;

        foreach (var (element, row, column) in Cells())
        {
            if (column == 0)
            {
                x = RowStart(row);
                if (row > 0) y += _rowHeights[row - 1] + Spacing.Y;
            }

            float cellWidth = UniformColumns ? _columnWidths[column] : element.Size.X;
            var cell = new RectangleF(x, y, cellWidth, _rowHeights[row]);
            element.Position = AlignUtil.Place(element.Size, cell, HorizontalAlign, VerticalAlign) + element.CenterPoint;
            x += cellWidth + Spacing.X;
        }
    }

    private float RowStart(int row)
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
        DrawBackground(d);
        Group.Draw(d, d.Transform);
    }

    private void DrawBackground(Drawer d)
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
        Group.Dispose();
        _rowHeights.Clear();
        _rowWidths.Clear();
        _columnWidths.Clear();
    }
}
