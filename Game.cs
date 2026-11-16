using SDL3;

namespace microgl;

public class Game : IDisposable
{
    private readonly string _title;
    private readonly (int, int) _winSize;
    private nint _windowHandle;
    private GPURenderer _renderer;
    private readonly IGame _game;

    // Drawing tools
    private Drawer _drawer;

    public Game(string title, int w, int h, IGame game)
    {
        _title = title;
        _winSize = (w, h);
        _game = game;
    }

    public void Start()
    {
        if (!SDL.Init(SDL.InitFlags.Video))
        {
            throw new Exception($"Can't Init SDL: {SDL.GetError()}");
        }

        var (w, h) = _winSize;
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

        // Make pixel perfect rendering without uggly mipmapping
        SDL.SetDefaultTextureScaleMode(renderer, SDL.ScaleMode.Nearest);

        // Setup resizing logic
        SDL.SetRenderLogicalPresentation(renderer, w, h, SDL.RendererLogicalPresentation.Letterbox);

        // Initialization
        _drawer = new Drawer(_renderer);
        _game.Init(_drawer);

        // Game loop
        bool running = true;
        bool gotEvent;
        SDL.Event _event;
        float deltaTime = 0;
        while (running)
        {
            // Count time
            deltaTime = Time.Tick();

            gotEvent = false;
            while (SDL.PollEvent(out _event))
            {
                // Process most events
                _processEvents(ref _event, ref running);

                // When QUIT event
                if (!running)
                {
                    Dispose();
                    break;
                }

                gotEvent = true;
                _game.Update(_drawer, ref _event, deltaTime);
            }

            // Check for running broke
            if (!running)
            {
                Dispose();
                break;
            }

            // When no events
            if (!gotEvent)
            {
                _event = default;
                _game.Update(_drawer, ref _event, deltaTime);
            }

            _drawer.Reset();
            _game.Draw(_drawer, deltaTime);
            _renderer.RenderPresent();
        }

        SDL.DestroyWindow(_windowHandle);
        SDL.Quit();
    }

    private void _processEvents(ref SDL.Event ev, ref bool running)
    {
        var type = ev.Type;

        // Process exit
        if (type == (uint)SDL.EventType.Quit) running = false;

        // Processing Mouse
        else if (Mouse.IsMouseEvent(ref ev)) Mouse.ProcessEvent(_drawer, ref ev);

        // Processing Keyboard
        else if (Keyboard.IsKeyboardEvent(ref ev)) Keyboard.ProcessEvent(ref ev);
    }

    public void Dispose()
    {
        _game.Dispose();
        _drawer.Dispose();
        SDL.DestroyWindow(_windowHandle);
    }
}
