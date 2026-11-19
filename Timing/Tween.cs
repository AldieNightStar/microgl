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
    public Action? OnComplete { get; set; }

    public bool IsFinished => Elapsed >= Duration;
    public float Progress => Duration <= 0f ? 1f : Math.Clamp(Elapsed / Duration, 0f, 1f);
    public float CurrentValue { get; private set; }

    public Tween(
        float from,
        float to,
        float duration,
        Action<float> onUpdate,
        Func<float, float>? ease = null)
    {
        From = from;
        To = to;
        Duration = duration;
        OnUpdate = onUpdate;
        Ease = ease ?? Easing.Linear;
        CurrentValue = from;
    }

    public bool Update(float dt)
    {
        if (IsPaused || IsFinished)
            return false;

        Elapsed += dt;

        float t = Progress;
        float easedT = Ease(t);
        CurrentValue = From + (To - From) * easedT;

        OnUpdate?.Invoke(CurrentValue);

        if (Elapsed >= Duration)
        {
            Elapsed = Duration;
            OnComplete?.Invoke();
            return true;
        }

        return false;
    }

    public void Reset()
    {
        Elapsed = 0f;
        CurrentValue = From;
    }

    public void Dispose()
    {
        IsPaused = true;
        OnUpdate = null;
        OnComplete = null;
        Reset();
    }

    void IGameUpdatable.Update(float dt) => Update(dt);
}