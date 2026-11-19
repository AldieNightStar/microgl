namespace microgl;

public static class Easing
{
    private const float PI = MathF.PI;
    private const float C1 = 1.70158f;
    private const float C2 = C1 * 1.525f;
    private const float C3 = C1 + 1f;
    private const float C4 = (2f * PI) / 3f;
    private const float C5 = (2f * PI) / 4.5f;

    // Linear
    public static float Linear(float t) => t;

    // Sine
    public static float EaseInSine(float t) => 1f - MathF.Cos((t * PI) / 2f);
    public static float EaseOutSine(float t) => MathF.Sin((t * PI) / 2f);
    public static float EaseInOutSine(float t) => -(MathF.Cos(PI * t) - 1f) / 2f;

    // Quad
    public static float EaseInQuad(float t) => t * t;
    public static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    public static float EaseInOutQuad(float t) =>
        t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f;

    // Cubic
    public static float EaseInCubic(float t) => t * t * t;
    public static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);
    public static float EaseInOutCubic(float t) =>
        t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;

    // Quart
    public static float EaseInQuart(float t) => t * t * t * t;
    public static float EaseOutQuart(float t) => 1f - MathF.Pow(1f - t, 4f);
    public static float EaseInOutQuart(float t) =>
        t < 0.5f ? 8f * t * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 4f) / 2f;

    // Quint
    public static float EaseInQuint(float t) => t * t * t * t * t;
    public static float EaseOutQuint(float t) => 1f - MathF.Pow(1f - t, 5f);
    public static float EaseInOutQuint(float t) =>
        t < 0.5f ? 16f * t * t * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 5f) / 2f;

    // Expo
    public static float EaseInExpo(float t) =>
        t == 0f ? 0f : MathF.Pow(2f, 10f * t - 10f);

    public static float EaseOutExpo(float t) =>
        t == 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);

    public static float EaseInOutExpo(float t) => t switch
    {
        0f => 0f,
        1f => 1f,
        < 0.5f => MathF.Pow(2f, 20f * t - 10f) / 2f,
        _ => (2f - MathF.Pow(2f, -20f * t + 10f)) / 2f
    };

    // Circ
    public static float EaseInCirc(float t) => 1f - MathF.Sqrt(1f - MathF.Pow(t, 2f));
    public static float EaseOutCirc(float t) => MathF.Sqrt(1f - MathF.Pow(t - 1f, 2f));
    public static float EaseInOutCirc(float t) =>
        t < 0.5f
            ? (1f - MathF.Sqrt(1f - MathF.Pow(2f * t, 2f))) / 2f
            : (MathF.Sqrt(1f - MathF.Pow(-2f * t + 2f, 2f)) + 1f) / 2f;

    // Back
    public static float EaseInBack(float t) => C3 * t * t * t - C1 * t * t;
    public static float EaseOutBack(float t) =>
        1f + C3 * MathF.Pow(t - 1f, 3f) + C1 * MathF.Pow(t - 1f, 2f);
    public static float EaseInOutBack(float t) =>
        t < 0.5f
            ? (MathF.Pow(2f * t, 2f) * ((C2 + 1f) * 2f * t - C2)) / 2f
            : (MathF.Pow(2f * t - 2f, 2f) * ((C2 + 1f) * (t * 2f - 2f) + C2) + 2f) / 2f;

    // Elastic
    public static float EaseInElastic(float t) => t switch
    {
        0f => 0f,
        1f => 1f,
        _ => -MathF.Pow(2f, 10f * t - 10f) * MathF.Sin((t * 10f - 10.75f) * C4)
    };

    public static float EaseOutElastic(float t) => t switch
    {
        0f => 0f,
        1f => 1f,
        _ => MathF.Pow(2f, -10f * t) * MathF.Sin((t * 10f - 0.75f) * C4) + 1f
    };

    public static float EaseInOutElastic(float t) => t switch
    {
        0f => 0f,
        1f => 1f,
        < 0.5f => -(MathF.Pow(2f, 20f * t - 10f) * MathF.Sin((20f * t - 11.125f) * C5)) / 2f,
        _ => (MathF.Pow(2f, -20f * t + 10f) * MathF.Sin((20f * t - 11.125f) * C5)) / 2f + 1f
    };

    // Bounce
    public static float EaseInBounce(float t) => 1f - EaseOutBounce(1f - t);

    public static float EaseOutBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (t < 1f / d1)
        {
            return n1 * t * t;
        }
        if (t < 2f / d1)
        {
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        }
        if (t < 2.5f / d1)
        {
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        }
        return n1 * (t -= 2.625f / d1) * t + 0.984375f;
    }

    public static float EaseInOutBounce(float t) =>
        t < 0.5f
            ? (1f - EaseOutBounce(1f - 2f * t)) / 2f
            : (1f + EaseOutBounce(2f * t - 1f)) / 2f;
}