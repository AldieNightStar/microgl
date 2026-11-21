namespace microgl;

public sealed class TweenSequence : ITween
{
    private readonly List<ITween> _steps;
    private int _index;

    public bool Repeat { get; set; }
    public float Duration => _steps.Sum(s => s.Duration);
    public float Elapsed => _steps.Sum(s => s.Elapsed);
    public float Progress => TweenUtil.Progress(Elapsed, Duration);
    public bool IsPaused { get; set; }
    public bool IsFinished { get; private set; }
    public bool IsActive => !IsPaused && !IsFinished;

    public event Action? OnComplete;

    public TweenSequence(IEnumerable<ITween> steps, bool repeat = false)
    {
        _steps = steps.ToList();
        Repeat = repeat;
    }

    public void Update(Drawer d) => Update(Time.DeltaTime);

    public float Update(float dt)
    {
        if (IsPaused)
            return 0f;
        if (IsFinished)
            return dt;

        float leftover = AdvanceSteps(dt);
        while (_index == _steps.Count)
        {
            CompleteCycle();
            if (IsFinished)
                return leftover;
            if (leftover <= 0f || Duration <= 0f)
                return 0f;
            leftover = AdvanceSteps(leftover);
        }

        return 0f;
    }

    public void Reset()
    {
        _index = 0;
        IsFinished = false;
        foreach (var step in _steps)
            step.Reset();
    }

    public void Dispose()
    {
        IsPaused = true;
        OnComplete = null;
        foreach (var step in _steps)
            step.Dispose();
    }

    private float AdvanceSteps(float dt)
    {
        while (_index < _steps.Count)
        {
            var step = _steps[_index];
            dt = step.Update(dt);
            if (!step.IsFinished)
                return 0f;
            _index++;
        }

        return dt;
    }

    private void CompleteCycle()
    {
        OnComplete?.Invoke();
        if (Repeat)
            Reset();
        else
            IsFinished = true;
    }
}
