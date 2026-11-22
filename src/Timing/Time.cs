using SDL3;

namespace microgl;

public static class Time
{
    private static ulong _previousCounter;
    private static readonly double _countsPerSecond = SDL.GetPerformanceFrequency();

    public static float DeltaTime { get; private set; }
    public static long Frame { get; private set; }
    public static float DeltaTimeMax { get; set; } = 0.25f;
    public static long EpochMillis => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    internal static float Tick()
    {
        Frame++;
        ulong currentCounter = SDL.GetPerformanceCounter();

        if (_isFirstTick())
        {
            _previousCounter = currentCounter;
            DeltaTime = 0.0f;
            return DeltaTime;
        }

        DeltaTime = _clampToMax(_secondsSince(_previousCounter, currentCounter));
        _previousCounter = currentCounter;
        return DeltaTime;
    }

    public static double GetTimeMillis() => SDL.GetPerformanceCounter() * 1000.0 / _countsPerSecond;

    private static bool _isFirstTick() => _previousCounter == 0;

    private static float _secondsSince(ulong previousCounter, ulong currentCounter) =>
        (float)((currentCounter - previousCounter) / _countsPerSecond);

    private static float _clampToMax(float seconds) =>
        seconds > DeltaTimeMax ? DeltaTimeMax : seconds;
}
