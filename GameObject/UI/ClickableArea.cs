using System;
using System.Drawing;
using System.Numerics;

namespace microgl;

public class ClickableArea : GameObject
{
    public Vector2 Size { get; set; }

    public Rectangle Bounds
    {
        get => new((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        set
        {
            Position = new Vector2(value.X, value.Y);
            Size = new Vector2(value.Width, value.Height);
        }
    }

    public bool IsHovered { get; private set; }
    public bool IsPressed { get; private set; }
    public bool Enabled { get; set; } = true;

    public event Action? OnClick;
    public event Action? OnHoverEnter;
    public event Action? OnHoverExit;

    private const uint LeftButton = 1;

    public ClickableArea(Rectangle rect, bool centerOrigin = false)
    {
        Position = new Vector2(rect.X, rect.Y);
        Size = new Vector2(rect.Width, rect.Height);

        if (centerOrigin)
            CenterPoint = Size / 2f;
    }

    public ClickableArea(Vector2 position, Vector2 size, bool centerOrigin = true)
    {
        Position = position;
        Size = size;

        if (centerOrigin)
            CenterPoint = size / 2f;
    }

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

    private bool IsInside(Vector2 screenPos)
    {
        Vector2 local = ToLocal(screenPos);
        return local.X >= 0f && local.X <= Size.X &&
               local.Y >= 0f && local.Y <= Size.Y;
    }

    private void HandleMouseMove(Vector2 pos)
    {
        if (!IsAlive || !Enabled) return;

        bool inside = IsInside(pos);

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

        if (IsInside(Mouse.Position))
            IsPressed = true;
    }

    private void HandleMouseUp(uint button)
    {
        if (!IsAlive || !Enabled || button != LeftButton) return;

        bool inside = IsInside(Mouse.Position);

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
