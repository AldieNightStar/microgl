# GameFont

A TrueType font loaded from a `.ttf` file. Use it with `Label`, `Button` or `Drawer.DrawText`.

New to this? See [Tutorial 1: First window and drawing](../Tutorials/01-FirstWindow.md).

### Notes
* **TrueType font wrapper** (`readonly struct` implementing `IDisposable`) backed by SDL3_ttf (`TTF_Font`).
* **Automatically initializes `TextEngine`** on load and throws an exception if the `.ttf` file fails to open.
* **Measures text pixel dimensions** via `MeasureText` and must be disposed once when no longer needed.

### Usage
```csharp
// Load a TTF font at the specified point size (FromTTF is an alias for the constructor)
new GameFont(string filename, float size);
GameFont.FromTTF(string filename, float size);

// Measure pixel width and height of a string (returns Vector2.Zero for null/empty text)
font.MeasureText(string text);

// Close the underlying native TTF font resource
font.Dispose();
```

### Example
```csharp
var font = GameFont.FromTTF("Days.ttf", 18);
Vector2 textSize = font.MeasureText("Hello World");
font.Dispose();
```
