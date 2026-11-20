using System;
using System.Drawing;
using System.Numerics;

namespace microgl;

public class Button : UIElement
{
    // Background (Rectangle or Texture)
    public Texture? BackgroundTexture { get; set; }
    public Color Color { get; set; } = Color.FromArgb(65, 75, 95);
    public Color HoverColor { get; set; } = Color.FromArgb(95, 110, 140);
    public Color PressedColor { get; set; } = Color.FromArgb(45, 52, 68);
    public Color BorderColor { get; set; } = Color.White;

    // Text & Font (delegated to Label)
    public Color TextColor
    {
        get => _textColor;
        set { _textColor = value; SyncLabel(); }
    }

    public GameFont Font
    {
        get => _font;
        set { _font = value; SyncLabel(); }
    }

    public string? Text
    {
        get => _text;
        set { _text = value; SyncLabel(); }
    }

    // Icon (optional)
    public Texture? Icon { get; set; }
    public Vector2 IconSize { get; set; } = new Vector2(20f, 20f);
    public float ContentGap { get; set; } = 6f;

    // Animations (customizable or null to disable)
    public Func<Button, TweenPlanner>? HoverAnimation { get; set; } =
        btn => TweenCollection.Pulse(btn, scaleMultiplier: 1.07f, duration: 0.2f);

    public Func<Button, TweenPlanner>? PressAnimation { get; set; } =
        btn => TweenCollection.ButtonPress(btn, pressScale: 0.88f, duration: 0.25f);

    // State & Events (delegated to ClickableArea)
    public bool IsHovered => _hitArea.IsHovered;
    public bool IsPressed => _hitArea.IsPressed;

    public bool Enabled
    {
        get => _hitArea.Enabled;
        set => _hitArea.Enabled = value;
    }

    public event Action? OnClick
    {
        add => _hitArea.OnClick += value;
        remove => _hitArea.OnClick -= value;
    }

    public event Action? OnHoverEnter
    {
        add => _hitArea.OnHoverEnter += value;
        remove => _hitArea.OnHoverEnter -= value;
    }

    public event Action? OnHoverExit
    {
        add => _hitArea.OnHoverExit += value;
        remove => _hitArea.OnHoverExit -= value;
    }

    private readonly ClickableArea _hitArea;
    private string? _text;
    private GameFont _font;
    private Color _textColor = Color.White;
    private Label? _label;

    private Tween? _activeTween;
    private Vector2 _baseScale = Vector2.One;

    public Button(
        GameFont font,
        Vector2 position,
        Vector2 size,
        string? text = null,
        Texture? icon = null,
        Texture? backgroundTexture = null,
        bool centerOrigin = true)
        : base(position, size, centerOrigin)
    {
        Icon = icon;
        BackgroundTexture = backgroundTexture;

        if (backgroundTexture != null)
        {
            Color = Color.White;
            HoverColor = Color.FromArgb(230, 230, 230);
            PressedColor = Color.FromArgb(180, 180, 180);
        }

        _hitArea = new ClickableArea(LocalBounds);

        _hitArea.OnHoverEnter += () =>
        {
            if (!_hitArea.IsPressed && HoverAnimation != null)
                PlayTween(HoverAnimation(this).ToTween());
        };

        _hitArea.OnClick += () =>
        {
            if (PressAnimation != null)
                PlayTween(PressAnimation(this).ToTween());
        };

        _font = font;
        _text = text;
        SyncLabel();
    }

    protected override void OnResized()
    {
        if (_hitArea != null)
            _hitArea.Size = Size;
    }

    private void SyncLabel()
    {
        if (string.IsNullOrEmpty(_text))
        {
            _label?.Dispose();
            _label = null;
            return;
        }

        if (_label == null)
        {
            _label = new Label(_font, Vector2.Zero, _text, _textColor);
        }
        else
        {
            _label.Font = _font;
            _label.Text = _text;
            _label.TextColor = _textColor;
        }
    }

    protected override void OnInit(Drawer d)
    {
        _baseScale = Scale;
        _hitArea.Init(d, d.WorldTransform);
        _label?.Init(d);
    }

    protected override void OnUpdate(Drawer d)
    {
        _hitArea.IsAlive = IsAlive;
        _hitArea.Update(d, d.WorldTransform);

        _activeTween?.Update(d);
        _label?.Update(d);
    }

    protected override void OnDraw(Drawer d)
    {
        int w = (int)Size.X;
        int h = (int)Size.Y;
        var rect = LocalBounds;

        Color fillColor = !Enabled ? Color.FromArgb(45, 45, 50)
                        : IsPressed ? PressedColor
                        : IsHovered ? HoverColor
                        : Color;

        // 1. Background (Texture OR Rectangle + Border)
        if (BackgroundTexture != null)
        {
            d.BlendColor = fillColor;
            d.DrawTexture(BackgroundTexture, rect);
            d.BlendColor = BlendColor;
        }
        else
        {
            d.Color = fillColor;
            d.DrawRect(rect);

            d.Color = BorderColor;
            d.DrawRect(new Rectangle(0, 0, w, 2));
            d.DrawRect(new Rectangle(0, h - 2, w, 2));
            d.DrawRect(new Rectangle(0, 0, 2, h));
            d.DrawRect(new Rectangle(w - 2, 0, 2, h));
        }

        // 2. Icon & Label Layout
        bool hasIcon = Icon != null;
        bool hasLabel = _label != null && _label.Size.X > 0f;

        if (hasIcon && hasLabel)
        {
            var textSize = _label!.Size;
            float totalWidth = IconSize.X + ContentGap + textSize.X;
            float startX = MathF.Max(8f, (w - totalWidth) / 2f);

            float iconY = (h - IconSize.Y) / 2f;
            d.BlendColor = BlendColor;
            d.DrawTexture(Icon!, new Rectangle((int)startX, (int)iconY, (int)IconSize.X, (int)IconSize.Y));

            _label.Position = new Vector2(startX + IconSize.X + ContentGap, (h - textSize.Y) / 2f);
            d.DrawObject(_label);
        }
        else if (hasIcon)
        {
            float iconX = (w - IconSize.X) / 2f;
            float iconY = (h - IconSize.Y) / 2f;
            d.BlendColor = BlendColor;
            d.DrawTexture(Icon!, new Rectangle((int)iconX, (int)iconY, (int)IconSize.X, (int)IconSize.Y));
        }
        else if (hasLabel)
        {
            var textSize = _label!.Size;
            float textX = MathF.Max(6f, (w - textSize.X) / 2f);
            float textY = (h - textSize.Y) / 2f;

            _label.Position = new Vector2(textX, textY);
            d.DrawObject(_label);
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
        _hitArea.Dispose();
        _label?.Dispose();
        _label = null;
        _activeTween?.Dispose();
    }
}
