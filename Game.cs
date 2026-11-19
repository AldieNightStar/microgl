using System.Numerics;
using SDL3;

namespace microgl;

public class Game : IDisposable
{
    private readonly string _title;
    private readonly Vector2 _winSize;
    private nint _windowHandle;
    private GPURenderer _renderer;
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

        var (w, h) = ((int) _winSize.X, (int) _winSize.Y);
        var winFlags = SDL.WindowFlags.Resizable;
        if (!SDL.CreateWindowAndRenderer(_title, w, h, winFlags, out _windowHandle, out var renderer))
        {
            throw new Exception($"Can't open window: {SDL.GetError()}");
        }
        _renderer = new GPURenderer(renderer);

        if (_windowHandle == IntPtr.Zero)
        {
            SDL.LogError(SDL.LogCategory.Application, $"Can't open window: {SDL.GetError()}");
            SDL.Quit();
            return;
        }

        // Make pixel perfect rendering without ugly mipmapping
        SDL.SetDefaultTextureScaleMode(renderer, SDL.ScaleMode.Nearest);

        // Setup resizing logic
        SDL.SetRenderLogicalPresentation(renderer, w, h, SDL.RendererLogicalPresentation.Letterbox);

        // Initialization
        _drawer = new Drawer(_renderer)
        {
            ScreenSize=_winSize,
        };
        _game.Init(_drawer);

        // Game loop
        bool running = true;
        bool gotEvent;
        SDL.Event _event;
        float deltaTime;
        while (running)
        {
            deltaTime = Time.Tick();

            gotEvent = false;
            while (SDL.PollEvent(out _event))
            {
                _processEvents(ref _event, ref running);

                if (!running)
                    break;

                gotEvent = true;
                _game.Update(_drawer, ref _event, deltaTime);
            }

            if (!running)
                break;

            if (!gotEvent)
            {
                _event = default;
                _game.Update(_drawer, ref _event, deltaTime);
            }

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
        Gamepad.DisposeAll();

        if (_windowHandle != IntPtr.Zero)
        {
            SDL.DestroyWindow(_windowHandle);
            _windowHandle = IntPtr.Zero;
        }
    }
}
