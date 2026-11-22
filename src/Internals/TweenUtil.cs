namespace microgl;

internal static class TweenUtil
{
    public static float Progress(float elapsed, float duration) =>
        duration <= 0f ? 1f : Math.Clamp(elapsed / duration, 0f, 1f);

    public static float Overflow(float elapsed, float duration) =>
        MathF.Max(0f, elapsed - duration);
}
