using System.Numerics;

namespace microgl;

public class AnimatedTexture : IGameUpdatable
{
    private readonly AtlasTexture[] _frames;
    private int _frameIndex;
    private float _elapsed;

    public float FrameDuration { get; set; }
    public bool Loop { get; set; }
    public bool IsPaused { get; set; }
    public bool IsFinished { get; private set; }

    public event Action? OnComplete;

    public AnimatedTexture(AtlasTexture[] frames, float frameDuration, bool loop = true)
    {
        EnsureSameSize(frames);
        _frames = frames;
        FrameDuration = frameDuration;
        Loop = loop;
    }

    public int FrameCount => _frames.Length;
    public AtlasTexture CurrentFrame => _frames[_frameIndex];
    public Vector2 Size => _frames[0].Size;
    private int LastFrame => _frames.Length - 1;

    public int FrameIndex
    {
        get => _frameIndex;
        set
        {
            _frameIndex = Math.Clamp(value, 0, LastFrame);
            _elapsed = 0f;
            IsFinished = false;
        }
    }

    public void Update(float dt)
    {
        if (IsPaused || IsFinished || FrameDuration <= 0f) return;

        _elapsed += dt;
        while (_elapsed >= FrameDuration && !IsFinished)
        {
            _elapsed -= FrameDuration;
            NextFrame();
        }
    }

    public void Reset() => FrameIndex = 0;

    public void Dispose()
    {
        IsPaused = true;
        OnComplete = null;
    }

    void IGameUpdatable.Update(Drawer d) => Update(Time.DeltaTime);

    private void NextFrame()
    {
        if (_frameIndex < LastFrame)
        {
            _frameIndex++;
            return;
        }

        if (Loop) _frameIndex = 0;
        else IsFinished = true;
        OnComplete?.Invoke();
    }

    private static void EnsureSameSize(AtlasTexture[] frames)
    {
        if (frames.Length == 0)
            throw new ArgumentException("AnimatedTexture needs at least one frame.", nameof(frames));

        var size = frames[0].Size;
        if (frames.Any(f => f.Size != size))
            throw new ArgumentException($"All frames must be {size.X}x{size.Y}.", nameof(frames));
    }
}
