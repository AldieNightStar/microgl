using System.Numerics;
using SDL3;

namespace microgl;

public class Game : IDisposable
{
    private const int _vsyncEveryFrame = 1;
    private const int _vsyncDisabled = 0;
    private const int _eventBufferCapacity = 64;

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
        try
        {
            _initSdl();
            _createWindowOrThrow();
            _initGpuDeviceAndRenderer();
            _configureRenderer();

            _drawer = new Drawer(_renderer) { ScreenSize = _windowSize };
            _game.Init(_drawer);

            _runGameLoop();
        }
        finally
        {
            Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Gamepad.DisposeAll();
        _game.Dispose();
        TextEngine.Instance.Dispose();
        _renderer?.Dispose();

        _releaseGpuDevice();
        _releaseWindow();
        SDL.Quit();
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

        throw new Exception($"Can't create GPU Device: {SDL.GetError()}");
    }

    private void _createRendererOrThrow()
    {
        var rendererHandle = SDL.CreateGPURenderer(_gpuDevice, _windowHandle);
        if (rendererHandle == IntPtr.Zero)
        {
            throw new Exception($"Can't create GPU Renderer: {SDL.GetError()}");
        }

        _renderer = new GPURenderer(rendererHandle);
    }

    private void _configureRenderer()
    {
        var (width, height) = _windowDimensions();
        var rendererPointer = _renderer.GetNativePointer();

        SDL.SetDefaultTextureScaleMode(rendererPointer, SDL.ScaleMode.Nearest);
        SDL.SetRenderVSync(rendererPointer, VSync ? _vsyncEveryFrame : _vsyncDisabled);

        // The swapchain dimensions must exist before the logical presentation is set.
        SDL.RenderPresent(rendererPointer);
        SDL.SetRenderLogicalPresentation(rendererPointer, width, height, SDL.RendererLogicalPresentation.Letterbox);
    }

    private void _runGameLoop()
    {
        var events = new EventBuffer(_eventBufferCapacity);

        while (true)
        {
            float deltaTime = Time.Tick();

            if (!_pumpEvents(ref events)) return;

            _game.Update(_drawer, events.Flush(), deltaTime);
            _drawer.Reset();
            _game.Draw(_drawer, deltaTime);
            _renderer.RenderPresent();
        }
    }

    private bool _pumpEvents(ref EventBuffer events)
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
        else if (sdlEvent.Type == (uint)SDL.EventType.WindowFocusLost) _releaseHeldInput();
    }

    private static void _releaseHeldInput()
    {
        Keyboard.ReleaseAll();
        Mouse.ReleaseAll();
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
}
