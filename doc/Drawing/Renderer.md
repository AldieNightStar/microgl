# IRenderer / GPURenderer

The drawing backend under `Drawer`. `Game` creates a `GPURenderer` for you. Every draw, including textures baked with `Texture.FromDrawing` and `DynamicTexture`, runs on this renderer.

### Notes
* **`IRenderer`** is the common interface (`IDisposable`). It gives the native pointer, presents the frame, returns the GPU copy of a `Texture`, and sets a `Shader`.
* **`GPURenderer`** wraps an existing SDL GPU renderer. It keeps a GPU copy of each `Texture` it has drawn, and frees the copy when that `Texture` is disposed.
* **Dispose** releases the cached GPU copies and the native renderer.

### Usage
```csharp
renderer.GetNativePointer();
renderer.GetNativeTexture(Texture texture);
renderer.RenderPresent();
renderer.SetShader(Shader? shader);
renderer.Dispose();

var gpu = new GPURenderer(nint sdlRenderer);
gpu.IsValid(); // GPURenderer only: false once the native renderer is released
```

### Example
Get the renderer from the `Drawer` you received in `Init`, `Update` or `Draw`:
```csharp
Texture baked = Texture.FromDrawing(drawer.Renderer, 64, 64, d =>
{
    d.Color = Color.Red;
    d.DrawRect(new Rectangle(0, 0, 32, 32));
});
```
