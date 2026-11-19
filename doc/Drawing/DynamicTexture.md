# DynamicTexture

### Notes
* **Mutable procedural texture wrapper** implementing `IDisposable` that bakes drawing commands into an offscreen `Texture`.
* **Uses lazy generation**: the underlying `Texture` is only created on first access to the `Texture` property or when `UpdateTexture()` is explicitly called.
* **Re-renders cleanly on demand** by disposing the previous `Texture` and generating a fresh `Width x Height` texture via `Texture.FromDrawing`.

### Usage
```csharp
// Create a dynamic texture of size w x h with a drawing callback
new DynamicTexture(ushort w, ushort h, Action<Drawer> drawing);

// Read pixel dimensions or access the baked Texture (triggers lazy generation if not yet built)
dynTex.Width;
dynTex.Height;
dynTex.Texture;

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

dynTex.Dispose();
```
