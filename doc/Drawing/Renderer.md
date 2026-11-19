# IRenderer / GPURenderer / SoftwareRenderer

### Notes
* **`IRenderer` abstraction** (`IDisposable`) unifying native SDL3 rendering backends and GPU texture caching.
* **`GPURenderer`** wraps an existing hardware-accelerated `SDL_Renderer` pointer and caches uploaded textures via `TextureCache`.
* **`SoftwareRenderer`** renders offscreen into an in-memory `Surface` of size `w x h` and can bake the output into a `Texture` via `ProduceTexture`.

### Usage
```csharp
// Common IRenderer operations
nint ptr = renderer.GetNativePointer();
nint texPtr = renderer.GetNativeTexture(Texture texture);
bool valid = renderer.IsValid();
renderer.RenderPresent();
renderer.Dispose();

// Hardware GPU renderer wrapping an existing SDL_Renderer pointer
new GPURenderer(nint renderer);

// Offscreen CPU software renderer and texture baking
new SoftwareRenderer(ushort w, ushort h);
Texture baked = softRenderer.ProduceTexture(ushort w, ushort h);
```

### Example
```csharp
// Offscreen rendering with SoftwareRenderer
using var soft = new SoftwareRenderer(64, 64);
using var drawer = new Drawer(soft);
drawer.DrawRect(new Rectangle(0, 0, 32, 32));
Texture result = soft.ProduceTexture(64, 64);

// Hardware rendering with GPURenderer
using var gpu = new GPURenderer(sdlRendererPtr);
if (gpu.IsValid())
{
    gpu.RenderPresent();
}
```
