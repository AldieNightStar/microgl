namespace microgl;

public class GameTimer : IGameUpdatable
{
    public float Duration { get; set; }
    public float Elapsed { get; private set; }
    public bool Loop { get; set; }
    public bool IsPaused { get; set; }

    public event Action? OnComplete;

    public GameTimer(float duration, bool loop)
    {
        Duration = duration;
        Loop = loop;
        Elapsed = 0f;
    }

    public bool IsFinished => Elapsed >= Duration;
    public float Progress => Duration <= 0f ? 1f : Math.Clamp(Elapsed / Duration, 0f, 1f);
    public float Remaining => MathF.Max(0f, Duration - Elapsed);

    public bool Update(float dt)
    {
        if (IsPaused || Duration <= 0f)
            return false;

        if (!Loop && Elapsed >= Duration)
            return false;

        Elapsed += dt;

        if (Elapsed >= Duration)
        {
            if (Loop)
                Elapsed %= Duration;
            else
                Elapsed = Duration;

            OnComplete?.Invoke();
            return true;
        }

        return false;
    }

    public void Reset() => Elapsed = 0f;

    public void Restart(float newDuration)
    {
        Duration = newDuration;
        Elapsed = 0f;
    }

    public void Dispose()
    {
        IsPaused = true;  
        OnComplete = null;
        Reset();
    }

    void IGameUpdatable.Update(Drawer d) => Update(Time.DeltaTime);
}
