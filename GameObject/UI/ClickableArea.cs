using System;
using System.Drawing;
using System.Numerics;

namespace microgl;

public class ClickableArea : UIElement
{
    public bool IsHovered { get; private set; }
    public bool IsPressed { get; private set; }
    public bool Enabled { get; set; } = true;

    public event Action? OnClick;
    public event Action? OnHoverEnter;
    public event Action? OnHoverExit;

    private const uint LeftButton = 1;

    public ClickableArea(Rectangle rect, bool centerOrigin = false)
        : base(rect, centerOrigin) {}

    public ClickableArea(Vector2 position, Vector2 size, bool centerOrigin = true)
        : base(position, size, centerOrigin) { }

    protected override void OnInit(Drawer d)
    {
        Mouse.OnMove += HandleMouseMove;
        Mouse.OnDown += HandleMouseDown;
        Mouse.OnUp += HandleMouseUp;
    }

    protected override void OnUpdate(Drawer d, float dt)
    {
        if (!Enabled)
        {
            IsHovered = false;
            IsPressed = false;
        }
    }

    protected override void OnDraw(Drawer d, float dt)
    {
        // Invisible area: nothing to draw
    }

    private void HandleMouseMove(Vector2 pos)
    {
        if (!IsAlive || !Enabled) return;

        bool inside = ContainsPoint(pos);

        if (inside && !IsHovered)
        {
            IsHovered = true;
            OnHoverEnter?.Invoke();
        }
        else if (!inside && IsHovered)
        {
            IsHovered = false;
            IsPressed = false;
            OnHoverExit?.Invoke();
        }
    }

    private void HandleMouseDown(uint button)
    {
        if (!IsAlive || !Enabled || button != LeftButton) return;

        if (ContainsPoint(Mouse.Position))
            IsPressed = true;
    }

    private void HandleMouseUp(uint button)
    {
        if (!IsAlive || !Enabled || button != LeftButton) return;

        bool inside = ContainsPoint(Mouse.Position);

        if (IsPressed && inside)
            OnClick?.Invoke();

        IsPressed = false;
    }

    public override void Dispose()
    {
        Mouse.OnMove -= HandleMouseMove;
        Mouse.OnDown -= HandleMouseDown;
        Mouse.OnUp -= HandleMouseUp;

        OnClick = null;
        OnHoverEnter = null;
        OnHoverExit = null;
    }
}
