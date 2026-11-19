namespace microgl;

public sealed class TweenPlanner
{
    private readonly List<Tween> _steps = new();

    public TweenPlanner Call(
        float from,
        float to,
        float duration,
        Action<float> callback,
        Func<float, float>? ease = null)
    {
        var tween = new Tween(from, to, duration, callback, ease)
        {
            IsPaused = true
        };
        _steps.Add(tween);
        return this;
    }

    public TweenPlanner Wait(float seconds)
    {
        var waitTween = new Tween(0f, 0f, seconds, null)
        {
            IsPaused = true
        };
        _steps.Add(waitTween);
        return this;
    }

    public Tween ToTween()
    {
        if (_steps.Count == 0)
            return new Tween(0f, 0f, 0f, null);

        int index = 0;
        float totalDuration = 0f;

        for (int i = 0; i < _steps.Count; i++)
        {
            totalDuration += _steps[i].Duration;

            if (i < _steps.Count - 1)
            {
                var next = _steps[i + 1];

                _steps[i].OnComplete += overflow =>
                {
                    index++;
                    next.IsPaused = false;

                    if (overflow > 0f)
                        next.Update(overflow);
                };
            }
        }

        _steps[0].IsPaused = false;

        float lastElapsed = 0f;
        return new Tween(0f, totalDuration, totalDuration, elapsed =>
        {
            float dt = elapsed - lastElapsed;
            lastElapsed = elapsed;

            if (index < _steps.Count)
                _steps[index].Update(dt);
        });
    }

}
