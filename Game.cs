using SDL3;

namespace microgl;

public class Game : IDisposable
{
    private readonly string _title;
    private readonly (int, int) _winSize;
    private nint _windowHandle;
    private GPURenderer _renderer;
    private readonly IGame _game;

    // Delta time calc variables
    private ulong _lastTime;
    private double _deltaTime;

    // Drawing tools
    private Drawer _drawer;

    public Game(string title, int w, int h, IGame game)
    {
        _title = title;
        _winSize = (w, h);
        _game = game;
    }

    public void Dispose() { }

    public void Start()
    {
        if (!SDL.Init(SDL.InitFlags.Video))
        {
            throw new Exception($"Can't Init SDL: {SDL.GetError()}");
        }

        var (w, h) = _winSize;
        if (!SDL.CreateWindowAndRenderer(_title, w, h, 0, out _windowHandle, out var renderer))
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
        SDL.SetDefaultTextureScaleMode(_renderer.GetNativePointer(), SDL.ScaleMode.Nearest);

        // Initialization
        _drawer = new Drawer(_renderer);
        _game.Init(_drawer);

        // Game loop
        bool running = true;
        bool gotEvent;
        SDL.Event _event;
        while (running)
        {
            gotEvent = false;
            SDLCalcDelta(ref _deltaTime, ref _lastTime);
            while (SDL.PollEvent(out _event))
            {
                if (_event.Type == (uint) SDL.EventType.Quit)
                {
                    running = false;
                    break;
                }

                gotEvent = true;
                _game.Update(ref _event, _deltaTime);
            }

            // When no events
            if (!gotEvent)
            {
                _event = default;
                _game.Update(ref _event, _deltaTime);
            }

            _drawer.Reset();
            _game.Draw(_drawer, _deltaTime);
            _renderer.RenderPresent();
        }

        SDL.DestroyWindow(_windowHandle);
        SDL.Quit();
    }

    private static void SDLCalcDelta(ref double dt, ref ulong lastTime)
    {
        var currentTime = SDL.GetTicks();
        dt = ((float)(currentTime - lastTime)) / 1000;
        lastTime = currentTime;
    }
}
