# Label

### Notes
* **Text UI element** inheriting from `UIElement` that renders strings using a `GameFont`.
* **Caches rendered glyphs in a `DynamicTexture`** and automatically recalculates `Size` and `CenterPoint` whenever `Text`, `Font`, or `TextColor` changes.
* **Associated enums**: `TextAlign` (`Left`, `Center`, `Right`) and `VerticalAlign` (`Top`, `Middle`, `Bottom`) are defined alongside `Label`.

### Usage
```csharp
// Create a label with font, position, initial text, optional color (default White), and optional center origin
new Label(GameFont font, Vector2 position, string text, Color? color = null, bool centerOrigin = false);

// Update displayed text, font, or text color (automatically refreshes cached texture and Size)
label.Text;
label.Font;
label.TextColor;

// Standard UIElement lifecycle methods and resource cleanup
label.Init(Drawer d);
label.Update(Drawer d);
label.Draw(Drawer d);
label.Dispose();
```

### Example
```csharp
var label = new Label(font, new Vector2(360f, 100f), "Score: 0", Color.Gold, centerOrigin: true);
label.Init(d);

label.Text = "Score: 100";
label.TextColor = Color.White;

label.Update(d);
label.Draw(d);
label.Dispose();
```
