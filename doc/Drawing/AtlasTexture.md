# AtlasTexture

### Notes
* **Lightweight `readonly struct` view** representing a rectangular sub-region (`source`) inside a parent `Texture` without duplicating pixel data.
* **Supports grid slicing** from an entire texture (`FromTexture`), a specific sub-region (`FromRegion`), or an existing atlas slice (`SliceGrid`), with optional `spacing` and `margin`.
* **Does not own the underlying `Texture`**: disposing the parent `Texture` frees the shared image memory.
* **Validates input**: `SliceGrid` throws for non-positive tile sizes or negative spacing, and `SubRegion` throws when the region falls outside the atlas bounds.

### Usage
```csharp
// Create a single sub-texture view from a parent Texture and source Rectangle
new AtlasTexture(Texture atlas, Rectangle source);

// Slice an entire Texture or a specific Rectangle region into a row-major grid of tiles
AtlasTexture.FromTexture(Texture t, Vector2 tileSize, int spacing = 0, int margin = 0);
AtlasTexture.FromRegion(Texture t, Rectangle region, Vector2 tileSize, int spacing = 0);

// Further slice an existing AtlasTexture into a grid or extract a nested sub-region
atlasTex.SliceGrid(Vector2 tileSize, int spacing = 0);
atlasTex.SubRegion(Rectangle relativeRect);

// Read the parent Texture, sub-region dimensions (Vector2) or source Rectangle inside the parent Texture
atlasTex.Texture;
atlasTex.Size;
atlasTex.Bounds;
```

### Example
```csharp
var sheet = Texture.FromPNG("tileset.png");

// Slice 16x16 tiles with 1px spacing and 2px outer margin
AtlasTexture[] tiles = AtlasTexture.FromTexture(sheet, new Vector2(16f, 16f), spacing: 1, margin: 2);

// Extract a single sprite region and a nested sub-region
var panel = new AtlasTexture(sheet, new Rectangle(0, 64, 48, 48));
var corner = panel.SubRegion(new Rectangle(0, 0, 8, 8));
```
