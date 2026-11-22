using System.Numerics;

namespace microgl;

public class Tween : ITween
{
    public float From { get; set; }
    public float To { get; set; }
    public float Duration { get; set; }
    public float Elapsed { get; private set; }
    public float Progress => TweenUtil.Progress(Elapsed, Duration);
    public float CurrentValue { get; private set; }
    public bool IsPaused { get; set; }
    public bool IsFinished { get; private set; }
    public bool IsActive => !IsPaused && !IsFinished;

    public Func<float, float> Ease { get; set; }
    public Action<float>? OnUpdate { get; set; }
    public event Action? OnComplete;

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

    public void Update(Drawer d) => Update(Time.DeltaTime);

    public float Update(float dt)
    {
        if (IsPaused)
            return 0f;
        if (IsFinished)
            return dt;

        float leftover = TweenUtil.Overflow(Elapsed + dt, Duration);
        Elapsed = MathF.Min(Elapsed + dt, Duration);
        CurrentValue = From + (To - From) * Ease(Progress);
        OnUpdate?.Invoke(CurrentValue);

        if (Elapsed >= Duration)
        {
            IsFinished = true;
            OnComplete?.Invoke();
        }

        return leftover;
    }

    public void Reset()
    {
        Elapsed = 0f;
        CurrentValue = From;
        IsFinished = false;
    }

    public void Dispose()
    {
        IsPaused = true;
        OnUpdate = null;
        OnComplete = null;
        Reset();
    }

    public static TweenPlanner For(GameObject target) => new TweenPlanner().For(target);

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
