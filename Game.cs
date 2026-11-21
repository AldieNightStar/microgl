using System.Numerics;
using SDL3;

namespace microgl;

public class Game : IDisposable
{
    private const int VsyncEveryFrame = 1;
    private const int VsyncDisabled = 0;
    private const int EventBufferCapacity = 64;

    private readonly string _title;
    private readonly Vector2 _windowSize;
    private readonly IGame _game;
    private nint _windowHandle;
    private GPURenderer _renderer;
    private nint _gpuDevice;
    private Drawer _drawer;
    private bool _disposed;

    public bool VSync { get; set; } = true;

    public Game(string title, int width, int height, IGame game)
    {
        _title = title;
        _windowSize = new Vector2(width, height);
        _game = game;
    }

    public void Start()
    {
        _initSdl();
        _createWindowOrThrow();
        _initGpuDeviceAndRenderer();
        _configureRenderer();

        _drawer = new Drawer(_renderer) { ScreenSize = _windowSize };
        _game.Init(_drawer);

        _runGameLoop();

        Dispose();
        SDL.Quit();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _game.Dispose();
        _drawer.Dispose();
        _renderer?.Dispose();
        Gamepad.DisposeAll();

        _releaseGpuDevice();
        _releaseWindow();
    }

    private void _initSdl()
    {
        if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Gamepad))
            throw new Exception($"Can't Init SDL: {SDL.GetError()}");
    }

    private void _createWindowOrThrow()
    {
        var (width, height) = _windowDimensions();
        var windowFlags = SDL.WindowFlags.Resizable | SDL.WindowFlags.HighPixelDensity;

        _windowHandle = SDL.CreateWindow(_title, width, height, windowFlags);
        if (_windowHandle != IntPtr.Zero) return;

        SDL.Quit();
        throw new Exception($"Cannot init SDL window: {SDL.GetError()}");
    }

    private void _initGpuDeviceAndRenderer()
    {
        _createGpuDeviceOrThrow();
        _createRendererOrThrow();
    }

    private void _createGpuDeviceOrThrow()
    {
        var supportedFormats = SDL.GPUShaderFormat.SPIRV | SDL.GPUShaderFormat.DXIL | SDL.GPUShaderFormat.MSL;
        _gpuDevice = SDL.CreateGPUDevice(supportedFormats, false, null);
        if (_gpuDevice != IntPtr.Zero) return;

        _abortStartup();
        throw new Exception($"Can't create GPU Device: {SDL.GetError()}");
    }

    private void _createRendererOrThrow()
    {
        var rendererHandle = SDL.CreateGPURenderer(_gpuDevice, _windowHandle);
        if (rendererHandle == IntPtr.Zero)
        {
            SDL.DestroyGPUDevice(_gpuDevice);
            _abortStartup();
            throw new Exception($"Can't create GPU Renderer: {SDL.GetError()}");
        }

        _renderer = new GPURenderer(rendererHandle);
    }

    private void _configureRenderer()
    {
        var (width, height) = _windowDimensions();
        var rendererPointer = _renderer.GetNativePointer();

        SDL.SetDefaultTextureScaleMode(rendererPointer, SDL.ScaleMode.Nearest);
        SDL.SetRenderVSync(rendererPointer, VSync ? VsyncEveryFrame : VsyncDisabled);

        // The swapchain dimensions must exist before the logical presentation is set.
        SDL.RenderPresent(rendererPointer);
        SDL.SetRenderLogicalPresentation(rendererPointer, width, height, SDL.RendererLogicalPresentation.Letterbox);
    }

    private void _runGameLoop()
    {
        var events = new EventBuffer(EventBufferCapacity);

        while (true)
        {
            float deltaTime = Time.Tick();

            if (!_pumpEvents(events)) return;

            _game.Update(_drawer, events.Flush(), deltaTime);
            _drawer.Reset();
            _game.Draw(_drawer, deltaTime);
            _renderer.RenderPresent();
        }
    }

    /// <summary>Returns false once a quit event is received.</summary>
    private bool _pumpEvents(EventBuffer events)
    {
        while (SDL.PollEvent(out var sdlEvent))
        {
            if (_isQuitEvent(ref sdlEvent)) return false;

            _processInputEvent(ref sdlEvent);
            events.Add(ref sdlEvent);
        }

        return true;
    }

    private static bool _isQuitEvent(ref SDL.Event sdlEvent) => sdlEvent.Type == (uint)SDL.EventType.Quit;

    private void _processInputEvent(ref SDL.Event sdlEvent)
    {
        if (Mouse.IsMouseEvent(ref sdlEvent)) Mouse.ProcessEvent(_drawer, ref sdlEvent);
        else if (Keyboard.IsKeyboardEvent(ref sdlEvent)) Keyboard.ProcessEvent(ref sdlEvent);
        else if (Gamepad.IsGamepadEvent(ref sdlEvent)) Gamepad.ProcessEvent(ref sdlEvent);
    }

    private (int Width, int Height) _windowDimensions() => ((int)_windowSize.X, (int)_windowSize.Y);

    private void _releaseGpuDevice()
    {
        if (_gpuDevice == IntPtr.Zero) return;

        SDL.DestroyGPUDevice(_gpuDevice);
        _gpuDevice = IntPtr.Zero;
    }

    private void _releaseWindow()
    {
        if (_windowHandle == IntPtr.Zero) return;

        SDL.DestroyWindow(_windowHandle);
        _windowHandle = IntPtr.Zero;
    }

    private void _abortStartup()
    {
        SDL.DestroyWindow(_windowHandle);
        SDL.Quit();
    }
}
