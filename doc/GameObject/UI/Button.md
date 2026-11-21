# Button

A clickable button with text, an optional icon or background image, and hover and press animations.

New to this? See [Tutorial 3: Buttons and menus](../../Tutorials/03-ButtonsAndMenus.md).

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
var play = new Button(font, new Vector2(360f, 240f), new Vector2(160f, 44f), "Play");
play.OnClick += () => StartGame();

play.Init(d);      // in Init
play.Update(d);    // in Update
play.Draw(d);      // in Draw
play.Dispose();    // in Dispose
```

`position` is the button's center, because `centerOrigin` defaults to `true`.

### Example: styling
```csharp
play.Color = Color.SeaGreen;
play.HoverColor = Color.MediumSeaGreen;
play.PressedColor = Color.DarkGreen;
play.Icon = Texture.FromPNG("play.png");

play.HoverAnimation = btn => TweenCollection.Wiggle(btn);
play.PressAnimation = null;
play.Enabled = false;
```

The default animations are `TweenCollection.Pulse` on hover and `TweenCollection.ButtonPress` on click. A disabled button is drawn grey and ignores the mouse.
