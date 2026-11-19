using SDL3;

namespace microgl;

public static class Time
{
    private static ulong _lastCounter;
    private static readonly double _frequency = SDL.GetPerformanceFrequency();

    public static float DeltaTime { get; private set; }
    public static float DeltaTimeMax { get; set; } = 0.25f;
    public static long EpochMillis => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    internal static float Tick()
    {
        ulong current = SDL.GetPerformanceCounter();

        if (_lastCounter == 0)
        {
            _lastCounter = current;
            DeltaTime = 0.0f;
            return 0.0f;
        }

        DeltaTime = (float)((current - _lastCounter) / _frequency);
        if (DeltaTime > DeltaTimeMax) DeltaTime = DeltaTimeMax;

        _lastCounter = current;
        return DeltaTime;
    }

    public static double GetTimeMillis()
    {
        return (SDL.GetPerformanceCounter() * 1000.0) / _frequency;
    }
}