namespace microgl;

public interface ITween : IGameUpdatable
{
    float Duration { get; }
    float Elapsed { get; }
    float Progress { get; }
    bool IsPaused { get; set; }
    bool IsFinished { get; }
    bool IsActive { get; }

    event Action? OnComplete;

    float Update(float dt);
    void Reset();
}
