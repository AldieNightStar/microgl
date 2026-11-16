using SDL3;

namespace microgl;

public static class Time
{
    private static ulong _lastCounter;
    private static readonly double _frequency = SDL.GetPerformanceFrequency();

    public static double DeltaTime { get; private set; }

    internal static double Tick()
    {
        ulong current = SDL.GetPerformanceCounter();

        if (_lastCounter == 0)
        {
            _lastCounter = current;
            DeltaTime = 0.0;
            return 0.0;
        }

        DeltaTime = (current - _lastCounter) / _frequency;
        _lastCounter = current;
        return DeltaTime;
    }

    public static double GetTimeMillis()
    {
        return (SDL.GetPerformanceCounter() * 1000.0) / _frequency;
    }
}