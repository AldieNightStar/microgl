using System.Numerics;

namespace microgl;

public class Tween : IGameUpdatable
{
    public float From { get; set; }
    public float To { get; set; }
    public float Duration { get; set; }
    public float Elapsed { get; private set; }
    public bool IsPaused { get; set; }

    public Func<float, float> Ease { get; set; }
    public Action<float>? OnUpdate { get; set; }
    public Action<float> OnComplete { get; set; }

    private bool _completed;
    public bool IsFinished => _completed;

    public float Progress => Duration <= 0f ? 1f : Math.Clamp(Elapsed / Duration, 0f, 1f);
    public float CurrentValue { get; private set; }

    public Tween(
        float from,
        float to,
        float duration,
        Action<float>? onUpdate,
        Func<float, float>? ease = null)
    {
        From = from;
        To = to;
        Duration = duration;
        OnUpdate = onUpdate;
        Ease = ease ?? Easing.Linear;
        CurrentValue = from;
    }

    public void Update(float dt)
    {
        if (IsPaused || IsFinished)
            return;

        CurrentValue = _evaluate(dt);
        OnUpdate?.Invoke(CurrentValue);

        if (Elapsed >= Duration)
        {
            float overflow = Elapsed - Duration;
            Elapsed = Duration;
            _completed = true;
            OnComplete?.Invoke(overflow);
        }
    }

    public void Reset()
    {
        Elapsed = 0f;
        CurrentValue = From;
        _completed = false;
    }

    public void Dispose()
    {
        IsPaused = true;
        OnUpdate = null;
        OnComplete = null;
        Reset();
    }

    private float _evaluate(float dt)
    {
        Elapsed += dt;
        float t = Duration <= 0f ? 1f : Math.Clamp(Elapsed / Duration, 0f, 1f);
        return From + (To - From) * Ease(t);
    }


    public static TweenPlanner Call(
        float from,
        float to,
        float duration,
        Action<float> callback,
        Func<float, float>? ease = null
    ) => new TweenPlanner().Call(from, to, duration, callback, ease);

    public static TweenPlanner Call(
        Vector2 from,
        Vector2 to,
        float duration,
        Action<Vector2> callback,
        Func<float, float>? ease = null
    ) => new TweenPlanner().Call(from, to, duration, callback, ease);

    public static TweenPlanner CallFunc(Action callback) => new TweenPlanner().CallFunc(callback);

    public static TweenPlanner Wait(float seconds) => new TweenPlanner().Wait(seconds);
}
