using System.Numerics;
using SDL3;

namespace microgl;

public class Game : IDisposable
{
    private readonly string _title;
    private readonly Vector2 _winSize;
    private nint _windowHandle;
    private GPURenderer _renderer;
    private nint _gpuDevice;
    private readonly IGame _game;
    private bool _disposed;

    // Drawing tools
    private Drawer _drawer;

    public Game(string title, int w, int h, IGame game)
    {
        _title = title;
        _winSize = new Vector2(w, h);
        _game = game;
    }

    public void Start()
    {
        if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Gamepad))
        {
            throw new Exception($"Can't Init SDL: {SDL.GetError()}");
        }

        var (w, h) = ((int)_winSize.X, (int)_winSize.Y);
        var winFlags = SDL.WindowFlags.Resizable | SDL.WindowFlags.HighPixelDensity;

        _windowHandle = SDL.CreateWindow(_title, w, h, winFlags);
        if (_windowHandle == IntPtr.Zero)
        {
            SDL.Quit();
            throw new Exception($"Cannot init SDL window: {SDL.GetError()}");
        }

        // Init GPU renderer
        _initGpuDeviceAndRenderer();

        var rendererPtr = _renderer.GetNativePointer();

        // Make pixel perfect rendering without ugly mipmapping
        SDL.SetDefaultTextureScaleMode(rendererPtr, SDL.ScaleMode.Nearest);

        // Ensure swapchain dimensions are initialized before setting logical presentation
        SDL.RenderPresent(rendererPtr);
        SDL.SetRenderLogicalPresentation(rendererPtr, w, h, SDL.RendererLogicalPresentation.Letterbox);

        // Initialization
        _drawer = new Drawer(_renderer)
        {
            ScreenSize = _winSize,
        };
        _game.Init(_drawer);

        // Game loop
        bool running = true;
        SDL.Event _event;
        var events = new EventBuffer(64);
        float deltaTime;

        while (running)
        {
            deltaTime = Time.Tick();

            while (SDL.PollEvent(out _event))
            {
                _processEvents(ref _event, ref running);

                if (!running)
                    break;

                events.Add(ref _event);
            }

            if (!running)
                break;

            _game.Update(_drawer, events.Flush(), deltaTime);

            _drawer.Reset();
            _game.Draw(_drawer, deltaTime);
            _renderer.RenderPresent();
        }

        Dispose();
        SDL.Quit();
    }

    private void _processEvents(ref SDL.Event ev, ref bool running)
    {
        var type = ev.Type;

        if (type == (uint)SDL.EventType.Quit) running = false;
        else if (Mouse.IsMouseEvent(ref ev)) Mouse.ProcessEvent(_drawer, ref ev);
        else if (Keyboard.IsKeyboardEvent(ref ev)) Keyboard.ProcessEvent(ref ev);
        else if (Gamepad.IsGamepadEvent(ref ev)) Gamepad.ProcessEvent(ref ev);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _game.Dispose();
        _drawer.Dispose();
        _renderer?.Dispose();
        Gamepad.DisposeAll();

        if (_gpuDevice != IntPtr.Zero)
        {
            SDL.DestroyGPUDevice(_gpuDevice);
            _gpuDevice = IntPtr.Zero;
        }

        if (_windowHandle != IntPtr.Zero)
        {
            SDL.DestroyWindow(_windowHandle);
            _windowHandle = IntPtr.Zero;
        }
    }

    private void _initGpuDeviceAndRenderer()
    {
        var supportedFormats = SDL.GPUShaderFormat.SPIRV | SDL.GPUShaderFormat.DXIL | SDL.GPUShaderFormat.MSL;
        _gpuDevice = SDL.CreateGPUDevice(supportedFormats, false, null);
        if (_gpuDevice == IntPtr.Zero)
        {
            SDL.DestroyWindow(_windowHandle);
            SDL.Quit();
            throw new Exception($"Can't create GPU Device: {SDL.GetError()}");
        }

        var renderer = SDL.CreateGPURenderer(_gpuDevice, _windowHandle);
        if (renderer == IntPtr.Zero)
        {
            SDL.DestroyGPUDevice(_gpuDevice);
            SDL.DestroyWindow(_windowHandle);
            SDL.Quit();
            throw new Exception($"Can't create GPU Renderer: {SDL.GetError()}");
        }
        _renderer = new GPURenderer(renderer);
    }
}
