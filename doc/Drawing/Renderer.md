# IRenderer / GPURenderer / SoftwareRenderer

The low-level drawing backend. `Game` creates a `GPURenderer` for you, so you usually meet renderers only when you draw offscreen.

New to this? See [Tutorial 1: First window and drawing](../Tutorials/01-FirstWindow.md).

### Notes
* **`IRenderer` abstraction** (`IDisposable`) unifying native SDL3 rendering backends and GPU texture caching.
* **`GPURenderer`** wraps an existing hardware-accelerated `SDL_Renderer` pointer and caches uploaded textures via `ResourceCache`.
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

// GPU effects and render targets, used by Shader and MaskScope
renderer.SetShader(Shader? shader);
RenderTarget target = renderer.CreateTargetTexture(int width, int height);
RenderTarget? current = renderer.GetRenderTarget();   // null = the screen
renderer.SetRenderTarget(RenderTarget? target);
renderer.SetRenderTargetBlendMode(RenderTarget target, SDL.BlendMode mode);
renderer.RenderTargetToScreen(RenderTarget target, SDL.FRect dstRect);
target.Dispose();
```

`SoftwareRenderer.SetShader` throws `NotSupportedException`.

### Example
For drawing into a texture, [Texture.FromDrawing](Texture.md) does all of this for you. The manual way:
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
