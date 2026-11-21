using System.Numerics;

namespace microgl;

public sealed class TweenPlanner
{
    private readonly List<ITween> _steps = new();

    public TweenPlanner Call(
        float from,
        float to,
        float duration,
        Action<float> callback,
        Func<float, float>? ease = null)
    {
        return Add(new Tween(from, to, duration, callback, ease));
    }

    public TweenPlanner Call(
        Vector2 from,
        Vector2 to,
        float duration,
        Action<Vector2> callback,
        Func<float, float>? ease = null)
    {
        return Call(0f, 1f, duration, t => callback(Vector2.Lerp(from, to, t)), ease);
    }

    public TweenPlanner Wait(float seconds) => Add(new Tween(0f, 0f, seconds, null));

    public TweenPlanner CallFunc(Action callback)
    {
        var tween = new Tween(0f, 0f, 0f, null);
        tween.OnComplete += callback;
        return Add(tween);
    }

    public TweenPlanner Add(ITween tween)
    {
        _steps.Add(tween);
        return this;
    }

    public TweenPlanner Append(TweenPlanner other)
    {
        if (other == this)
            throw new InvalidOperationException("Cannot append a TweenPlanner to itself.");

        _steps.AddRange(other._steps);
        return this;
    }

    public TweenSequence ToTween(bool repeat = false) => new(_steps, repeat);
}
