using SDL3;

namespace microgl;

internal class TextEngine : IDisposable
{
    private readonly Dictionary<nint, nint> _engineCache = new();

    public nint GetEngineForRenderer(IRenderer renderer)
    {
        if (renderer == null || !renderer.IsValid())
            throw new ArgumentNullException(nameof(renderer), "Renderer is NULL or invalid");

        nint rendererPtr = renderer.GetNativePointer();
        if (rendererPtr == IntPtr.Zero)
            throw new ArgumentException("Renderer points to NULL", nameof(renderer));

        if (_engineCache.TryGetValue(rendererPtr, out nint cachedEngine))
            return cachedEngine;

        nint newEngine = _createNativeEngine(rendererPtr);

        // Only cache engines for persistent GPU renderers; temporary software engines shouldn't store
        if (renderer is GPURenderer)
        {
            _engineCache[rendererPtr] = newEngine;
        }

        return newEngine;
    }

    public void ClearCache()
    {
        foreach (nint enginePtr in _engineCache.Values)
        {
            if (enginePtr != IntPtr.Zero)
                TTF.DestroyRendererTextEngine(enginePtr);
        }

        _engineCache.Clear();
    }

    private static nint _createNativeEngine(nint rendererPtr)
    {
        nint engine = TTF.CreateRendererTextEngine(rendererPtr);
        if (engine == IntPtr.Zero)
            throw new Exception($"Unable to initialize SDL Font Engine: {SDL.GetError()}");

        return engine;
    }

    public void Dispose()
    {
        ClearCache();
    }

    public static void Init()
    {
        if (!TTF.Init()) throw new Exception($"SDL TTF wasn't initialized successfully: {SDL.GetError()}");
    }

    public static void FreeEngineForRenderer(nint enginePtr, IRenderer renderer)
    {
        if (renderer is GPURenderer) return;
        TTF.DestroyRendererTextEngine(enginePtr);
    }
}
