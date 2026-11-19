using System;
using System.Drawing;
using System.Numerics;

namespace microgl;

public class Button : GameObject
{
    public Vector2 Size { get; set; }

    // Colors
    public Color Color { get; set; } = Color.FromArgb(65, 75, 95);
    public Color HoverColor { get; set; } = Color.FromArgb(95, 110, 140);
    public Color PressedColor { get; set; } = Color.FromArgb(45, 52, 68);
    public Color BorderColor { get; set; } = Color.White;
    public Color TextColor { get; set; } = Color.White;
    public GameFont? Font { get; set; }

    // Content (optional)
    public string? Text { get; set; }
    public Texture? Icon { get; set; }
    public Vector2 IconSize { get; set; } = new Vector2(20f, 20f);
    public float ContentGap { get; set; } = 6f;

    // Animations (customizable or null to disable)
    public Func<Button, TweenPlanner>? HoverAnimation { get; set; } =
        btn => TweenCollection.Pulse(btn, scaleMultiplier: 1.07f, duration: 0.2f);

    public Func<Button, TweenPlanner>? PressAnimation { get; set; } =
        btn => TweenCollection.ButtonPress(btn, pressScale: 0.88f, duration: 0.25f);

    // State & Events
    public bool IsHovered { get; private set; }
    public bool IsPressed { get; private set; }
    public bool Enabled { get; set; } = true;

    public event Action? OnClick;
    public event Action? OnHoverEnter;
    public event Action? OnHoverExit;

    private Tween? _activeTween;
    private Color _currentFillColor;
    private Vector2 _baseScale = Vector2.One;
    private const uint LeftButton = 1;

    public Button(
        Vector2 position,
        Vector2 size,
        string? text = null,
        GameFont? font = null,
        Texture? icon = null,
        bool centerOrigin = true)
    {
        Position = position;
        Size = size;
        Text = text;
        Font = font;
        Icon = icon;

        if (centerOrigin)
            CenterPoint = size / 2f;

        _currentFillColor = Color;
    }

    protected override void OnInit(Drawer d)
    {
        _baseScale = Scale;
        _currentFillColor = Color;

        Mouse.OnMove += HandleMouseMove;
        Mouse.OnDown += HandleMouseDown;
        Mouse.OnUp += HandleMouseUp;
    }

    protected override void OnUpdate(Drawer d, float dt)
    {
        _activeTween?.Update(dt);

        if (!Enabled)
        {
            IsHovered = false;
            IsPressed = false;
            _currentFillColor = Color.FromArgb(45, 45, 50);
        }
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
            if (!IsPressed)
            {
                _currentFillColor = HoverColor;
                if (HoverAnimation != null)
                    PlayTween(HoverAnimation(this).ToTween());
            }
            OnHoverEnter?.Invoke();
        }
        else if (!inside && IsHovered)
        {
            IsHovered = false;
            IsPressed = false;
            _currentFillColor = Color;
            OnHoverExit?.Invoke();
        }
    }

    private void HandleMouseDown(uint button)
    {
        if (!IsAlive || !Enabled || button != LeftButton) return;

        if (IsInside(Mouse.Position))
        {
            IsPressed = true;
            _currentFillColor = PressedColor;
        }
    }

    private void HandleMouseUp(uint button)
    {
        if (!IsAlive || !Enabled || button != LeftButton) return;

        bool inside = IsInside(Mouse.Position);

        if (IsPressed && inside)
        {
            if (PressAnimation != null)
                PlayTween(PressAnimation(this).ToTween());

            OnClick?.Invoke();
        }

        IsPressed = false;
        _currentFillColor = inside ? HoverColor : Color;
    }

    protected override void OnDraw(Drawer d, float dt)
    {
        int w = (int)Size.X;
        int h = (int)Size.Y;

        // 1. Background
        d.Color = _currentFillColor;
        d.DrawRect(new Rectangle(0, 0, w, h));

        // 2. Border
        d.Color = BorderColor;
        d.DrawRect(new Rectangle(0, 0, w, 2));
        d.DrawRect(new Rectangle(0, h - 2, w, 2));
        d.DrawRect(new Rectangle(0, 0, 2, h));
        d.DrawRect(new Rectangle(w - 2, 0, 2, h));

        // 3. Icon & Text Layout
        bool hasIcon = Icon != null;
        bool hasText = !string.IsNullOrEmpty(Text) && Font.HasValue;

        if (hasIcon && hasText)
        {
            var font = Font!.Value;
            var textSize = font.MeasureText(Text!);

            // Center the combined [Icon + Gap + Text] block horizontally
            float totalWidth = IconSize.X + ContentGap + textSize.X;
            float startX = MathF.Max(8f, (w - totalWidth) / 2f);

            float iconY = (h - IconSize.Y) / 2f;
            d.BlendColor = Color.White;
            d.DrawTexture(Icon!, new Rectangle((int)startX, (int)iconY, (int)IconSize.X, (int)IconSize.Y));

            d.Font = font;
            d.FontColor = TextColor;
            float textX = startX + IconSize.X + ContentGap;
            float textY = (h - textSize.Y) / 2f;
            d.DrawText(Text!, new Vector2(textX, textY));
        }
        else if (hasIcon)
        {
            float iconX = (w - IconSize.X) / 2f;
            float iconY = (h - IconSize.Y) / 2f;
            d.BlendColor = Color.White;
            d.DrawTexture(Icon!, new Rectangle((int)iconX, (int)iconY, (int)IconSize.X, (int)IconSize.Y));
        }
        else if (hasText)
        {
            var font = Font!.Value;
            var textSize = font.MeasureText(Text!);

            d.Font = font;
            d.FontColor = TextColor;
            float textX = MathF.Max(6f, (w - textSize.X) / 2f);
            float textY = (h - textSize.Y) / 2f;
            d.DrawText(Text!, new Vector2(textX, textY));
        }
    }

    private void PlayTween(Tween tween)
    {
        if (_activeTween?.IsActive == true)
        {
            tween.Dispose();
            return;
        }

        Scale = _baseScale;
        _activeTween?.Dispose();
        _activeTween = tween;
    }

    public override void Dispose()
    {
        Mouse.OnMove -= HandleMouseMove;
        Mouse.OnDown -= HandleMouseDown;
        Mouse.OnUp -= HandleMouseUp;

        _activeTween?.Dispose();
        OnClick = null;
        OnHoverEnter = null;
        OnHoverExit = null;
    }
}
