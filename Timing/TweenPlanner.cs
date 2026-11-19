using System.Numerics;

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

    public TweenPlanner Call(
        Vector2 from,
        Vector2 to,
        float duration,
        Action<Vector2> callback,
        Func<float, float>? ease = null)
    {
        return Call(0f, 1f, duration, t => callback(Vector2.Lerp(from, to, t)), ease);
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

    public TweenPlanner CallFunc(Action callback)
    {
        var funcTween = new Tween(0f, 0f, 0f, null)
        {
            IsPaused = true
        };
        funcTween.OnComplete += _ => callback();
        _steps.Add(funcTween);
        return this;
    }

    public TweenPlanner Append(TweenPlanner other)
    {
        if (other == this)
            throw new InvalidOperationException("Cannot append a TweenPlanner to itself.");

        _steps.AddRange(other._steps);
        return this;
    }

    public Tween ToTween(bool repeat = false)
    {
        if (_steps.Count == 0)
            return new Tween(0f, 0f, 0f, null) { Repeatable = repeat };

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
        var master = new Tween(0f, totalDuration, totalDuration, elapsed =>
        {
            float dt = elapsed - lastElapsed;
            lastElapsed = elapsed;

            if (index < _steps.Count)
                _steps[index].Update(dt);
        })
        {
            Repeatable = repeat
        };

        master.OnComplete += _ =>
        {
            if (master.Repeatable)
            {
                index = 0;
                lastElapsed = 0f;
                for (int i = 0; i < _steps.Count; i++)
                {
                    _steps[i].Reset();
                    _steps[i].IsPaused = i != 0;
                }
            }
        };

        return master;
    }

}
