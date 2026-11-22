using SDL3;

namespace microgl;

internal sealed class TextEngine : IDisposable
{
    internal static TextEngine Instance { get; } = new();

    private readonly ResourceCache<IRenderer, nint> _engines;

    private TextEngine()
    {
        _engines = new ResourceCache<IRenderer, nint>(_createEngine, TTF.DestroyRendererTextEngine);
    }

    public nint GetEngineForRenderer(IRenderer renderer) => _engines.Get(renderer);

    public void Dispose()
    {
        _engines.Dispose();
        TTF.Quit();
    }

    public static void Init()
    {
        if (!TTF.Init()) throw new Exception($"SDL TTF wasn't initialized successfully: {SDL.GetError()}");
    }

    private static nint _createEngine(IRenderer renderer)
    {
        nint engine = TTF.CreateRendererTextEngine(renderer.GetNativePointer());
        if (engine == IntPtr.Zero)
            throw new Exception($"Unable to initialize SDL Font Engine: {SDL.GetError()}");

        return engine;
    }
}
