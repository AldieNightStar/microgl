namespace microgl;

public sealed class TweenSequence : ITween
{
    private readonly List<ITween> _steps;
    private readonly List<GameObject> _targets;
    private int _index;

    public bool Repeat { get; set; }
    public float Duration => _totalDuration();
    public float Elapsed => _totalElapsed();
    public float Progress => TweenUtil.Progress(Elapsed, Duration);
    public bool IsPaused { get; set; }
    public bool IsFinished { get; private set; }
    public bool IsActive => !IsPaused && !IsFinished;

    public event Action? OnComplete;

    public TweenSequence(IEnumerable<ITween> steps, bool repeat = false, IEnumerable<GameObject>? targets = null)
    {
        _steps = steps.ToList();
        _targets = targets?.ToList() ?? new List<GameObject>();
        Repeat = repeat;
    }

    public void Update(Drawer d) => Update(Time.DeltaTime);

    public float Update(float dt)
    {
        if (IsPaused) return 0f;
        
        if (IsFinished) return dt;

        if (_hasDeadTarget())
        {
            IsFinished = true;
            return dt;
        }

        float leftover = _advanceSteps(dt);
        while (_index == _steps.Count)
        {
            _completeCycle();
            if (IsFinished) return leftover;
            if (leftover <= 0f || Duration <= 0f) return 0f;
            leftover = _advanceSteps(leftover);
        }

        return 0f;
    }

    public void Reset()
    {
        _index = 0;
        IsFinished = false;
        foreach (var step in _steps)
        {
            step.Reset();
        }
    }

    public void Dispose()
    {
        IsPaused = true;
        OnComplete = null;
        foreach (var step in _steps)
        {
            step.Dispose();
        }
    }

    private float _totalDuration()
    {
        float total = 0f;
        foreach (var step in _steps) total += step.Duration;
        return total;
    }

    private float _totalElapsed()
    {
        float total = 0f;
        foreach (var step in _steps) total += step.Elapsed;
        return total;
    }

    private float _advanceSteps(float dt)
    {
        while (_index < _steps.Count)
        {
            var step = _steps[_index];
            dt = step.Update(dt);
            if (!step.IsFinished)
            {
                return 0f;
            }
            _index++;
        }

        return dt;
    }

    private void _completeCycle()
    {
        OnComplete?.Invoke();
        if (Repeat)
        {
            Reset();
        }
        else
        {
            IsFinished = true;
        }
    }

    private bool _hasDeadTarget()
    {
        foreach (var target in _targets)
        {
            if (!target.IsAlive) return true;
        }
        return false;
    }
}
