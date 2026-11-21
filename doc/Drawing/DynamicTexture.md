# DynamicTexture

A texture you draw with code and redraw when its content changes, for example a score panel or a minimap.

New to this? See [Tutorial 1: First window and drawing](../Tutorials/01-FirstWindow.md).

### Notes
* **Mutable procedural texture wrapper** implementing `IDisposable` that bakes drawing commands into an offscreen `Texture`.
* **Uses lazy generation**: the underlying `Texture` is only created on first access to the `Texture` property, or when `UpdateTexture()` or `Resize()` is called.
* **Supports resizing and re-rendering on demand**: `Resize(w, h)` updates `Width` and `Height` and regenerates the underlying `Texture` only when the dimensions actually change.

### Usage
```csharp
// Create a dynamic texture of size w x h with a drawing callback
new DynamicTexture(ushort w, ushort h, Action<Drawer> drawing);

// Read pixel dimensions or access the baked Texture (triggers lazy generation if not yet built)
dynTex.Width;
dynTex.Height;
dynTex.Texture;

// Change dimensions (triggers UpdateTexture() if width or height changed)
dynTex.Resize(ushort w, ushort h);

// Re-run the drawing callback to regenerate the underlying Texture, or free resources
dynTex.UpdateTexture();
dynTex.Dispose();
```

### Example
```csharp
int score = 10;
var dynTex = new DynamicTexture(128, 32, d =>
{
    d.Font = font;
    d.DrawText(\$"Score: {score}", Vector2.Zero);
});

// Access baked texture and re-bake when data changes
Texture tex = dynTex.Texture;
score = 20;
dynTex.UpdateTexture();

// Resize when more space is needed
dynTex.Resize(256, 32);

dynTex.Dispose();
```
