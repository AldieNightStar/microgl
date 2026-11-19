# Texture

### Notes
* **2D image resource** wrapping an SDL `Surface` and implementing `IDisposable`.
* **Supports loading from image files** (`FromPNG`, `FromBMP`) as well as **procedural offscreen generation** via `FromDrawing` using a temporary `SoftwareRenderer` and `Drawer`.
* **Notifies `FreeingResourceBus`** on `Dispose()` before freeing the underlying `Surface` so cached GPU textures are cleaned up automatically.

### Usage
```csharp
// Wrap an existing Surface or load a texture from a PNG/BMP file
new Texture(Surface surface);
Texture.FromPNG(string filename);
Texture.FromBMP(string filename);

// Procedurally draw into a new texture of size w x h using a temporary Drawer
Texture.FromDrawing(ushort w, ushort h, Action<Drawer> act);

// Access the underlying Surface, pixel dimensions (Vector2), or release resources
texture.Surface;
texture.Size;
texture.Dispose();
```

### Example
```csharp
// Load from file
var sprite = Texture.FromPNG("player.png");
Vector2 size = sprite.Size;

// Or generate procedurally
var boxTex = Texture.FromDrawing(32, 32, d =>
{
    d.Color = Color.Red;
    d.DrawRect(new Rectangle(0, 0, 32, 32));
});

sprite.Dispose();
boxTex.Dispose();
```
