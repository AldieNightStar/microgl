# IRenderer / GPURenderer / SoftwareRenderer

The drawing backend under `Drawer`. `Game` creates a `GPURenderer` for you. Use `SoftwareRenderer` to draw into a texture.

### Notes
* **`IRenderer`** is the common interface (`IDisposable`). It gives the native pointer, presents the frame, returns the GPU copy of a `Texture`, and sets a `Shader`.
* **`GPURenderer`** wraps an existing SDL GPU renderer. It keeps a GPU copy of each `Texture` it has drawn, and frees the copy when that `Texture` is disposed.
* **`SoftwareRenderer`** draws into an in-memory `Surface` of size `w x h`. `ProduceTexture` copies the result into a new `Texture`. It does not support shaders.
* **Dispose** releases the cached GPU copies and the native renderer.

### Usage
```csharp
renderer.GetNativePointer();
renderer.GetNativeTexture(Texture texture);
renderer.RenderPresent();
renderer.IsValid();
renderer.SetShader(Shader? shader);
renderer.Dispose();

var gpu = new GPURenderer(nint sdlRenderer);

var soft = new SoftwareRenderer(ushort w, ushort h);
soft.Surface;
soft.SDLRenderer;
Texture baked = soft.ProduceTexture(ushort w, ushort h);
```

### Example
`Texture.FromDrawing` does this for you. The manual way:
```csharp
using var soft = new SoftwareRenderer(64, 64);
var drawer = new Drawer(soft);

drawer.DrawRect(new Rectangle(0, 0, 32, 32));
Texture result = soft.ProduceTexture(64, 64);
```
