# Button

### Notes
* **Interactive UI button** inheriting from `UIElement` that delegates hit-testing to `ClickableArea` and text rendering to `Label`.
* **Renders either as a bordered rectangle or a tinted `BackgroundTexture`**, automatically adjusting tint defaults when a texture is supplied.
* **Supports centered text, icons, or `[Icon + Gap + Text]` layouts**, plus customizable `HoverAnimation` and `PressAnimation` tweens.
* **Content stays inside the button**: whenever the text, font, icon or button size changes, the label's `AvailableSize` is set to the button size minus a 6px padding on each side and the icon space, so long text is cut at the button edge instead of overflowing. The hit area's `MaxSize` follows the button size too.

### Usage
```csharp
// Create a button with font, position, size, and optional text, icon, background texture, and center origin
new Button(GameFont font, Vector2 position, Vector2 size, string? text = null, Texture? icon = null, Texture? backgroundTexture = null, bool centerOrigin = true);

// Background styling (rectangle fill/border colors or custom texture)
button.BackgroundTexture; button.Color; button.HoverColor; button.PressedColor; button.BorderColor;

// Text, font, and optional icon layout
button.Text; button.Font; button.TextColor;
button.Icon; button.IconSize; button.ContentGap;

// Customizable tween animations (set to null to disable)
button.HoverAnimation; button.PressAnimation;

// Interaction state and events
button.IsHovered; button.IsPressed; button.Enabled;
button.OnClick; button.OnHoverEnter; button.OnHoverExit;

// Lifecycle methods
button.Init(Drawer d);
button.Update(Drawer d);
button.Draw(Drawer d);
button.Dispose();
```

### Example
```csharp
var btn = new Button(font, new Vector2(360f, 240f), new Vector2(160f, 44f), "Play");
btn.OnClick += () => Console.WriteLine("Clicked!");

btn.Init(d);
btn.Update(d);
btn.Draw(d);
btn.Dispose();
```
