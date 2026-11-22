using SDL3;

namespace microgl;

public sealed class Sound : IDisposable
{
    private const int _bitsPerByte = 8;
    private const int _bitsizeMask = 0xFF;

    private readonly SDL.AudioSpec _spec;
    private readonly nint _buffer;
    private readonly int _length;
    private readonly int _frameBytes;
    private readonly int _bytesPerSecond;

    private nint _stream;
    private bool _paused;
    private bool _disposed;
    private int _startOffset;
    private int _fedBytes;
    private float _volume = 1f;
    private float _speed = 1f;

    public Sound(string filename)
    {
        if (!SDL.LoadWAV(filename, out var spec, out var buffer, out uint length))
        {
            throw new Exception($"Sound \"{filename}\" wasn't loaded successfully: {SDL.GetError()}");
        }

        _spec = spec;
        _buffer = buffer;
        _length = (int)length;
        _frameBytes = _frameSize(spec);
        _bytesPerSecond = _frameBytes * spec.Freq;
    }

    public static Sound FromWAV(string filename) => new(filename);

    public bool Loop { get; set; }

    public float Duration => _secondsFromBytes(_length);

    public float Position
    {
        get => _secondsFromBytes(IsStopped ? _startOffset : _consumedBytes());
        set => _seekTo(_frameAlignedOffset(value));
    }

    public float Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            if (!IsStopped) SDL.SetAudioStreamGain(_stream, value);
        }
    }

    public float Speed
    {
        get => _speed;
        set
        {
            _speed = value;
            if (!IsStopped) SDL.SetAudioStreamFrequencyRatio(_stream, value);
        }
    }

    public bool IsPlaying => !IsStopped && !_paused;
    public bool IsPaused => !IsStopped && _paused;
    public bool IsStopped => _stream == IntPtr.Zero;

    public void Play()
    {
        _throwIfDisposed();
        if (IsPlaying) return;

        if (IsPaused) _resume();
        else _startStream(_startOffset);
    }

    public void Pause()
    {
        if (!IsPlaying) return;

        SDL.UnbindAudioStream(_stream);
        _paused = true;
    }

    public void Stop()
    {
        _destroyStream();
        _paused = false;
        _startOffset = 0;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        Stop();
        SDL.Free(_buffer);
    }

    internal void Update()
    {
        if (!IsPlaying || _queuedBytes() > 0) return;

        if (Loop) _queue(0, _length);
        else Stop();
    }

    private static int _frameSize(SDL.AudioSpec spec) =>
        ((int)spec.Format & _bitsizeMask) / _bitsPerByte * spec.Channels;

    private void _throwIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Sound));
    }

    private float _secondsFromBytes(int bytes) => bytes / (float)_bytesPerSecond;

    private int _queuedBytes() => SDL.GetAudioStreamQueued(_stream);

    private int _consumedBytes() => (_startOffset + _fedBytes - _queuedBytes()) % _length;

    private int _frameAlignedOffset(float seconds)
    {
        int frames = (int)(seconds * _bytesPerSecond / _frameBytes);
        return Math.Clamp(frames * _frameBytes, 0, _length - _frameBytes);
    }

    private void _seekTo(int offset)
    {
        if (IsStopped)
        {
            _startOffset = offset;
            return;
        }

        _destroyStream();
        _startStream(offset);
    }

    private void _startStream(int offset)
    {
        _stream = _createStream();
        _startOffset = offset;
        _fedBytes = 0;
        _queue(offset, _length - offset);
        AudioDevice.Track(this);

        if (!_paused) _bind();
    }

    private void _resume()
    {
        _paused = false;
        _bind();
    }

    private nint _createStream()
    {
        var sourceSpec = _spec;
        var deviceSpec = AudioDevice.Spec;
        var stream = SDL.CreateAudioStream(ref sourceSpec, ref deviceSpec);
        if (stream == IntPtr.Zero)
        {
            throw new Exception($"Can't create audio stream: {SDL.GetError()}");
        }

        SDL.SetAudioStreamGain(stream, _volume);
        SDL.SetAudioStreamFrequencyRatio(stream, _speed);
        return stream;
    }

    private void _bind()
    {
        if (!SDL.BindAudioStream(AudioDevice.Device, _stream))
        {
            throw new Exception($"Can't bind audio stream: {SDL.GetError()}");
        }
    }

    private void _queue(int offset, int length)
    {
        if (!SDL.PutAudioStreamData(_stream, _buffer + offset, length))
        {
            throw new Exception($"Can't queue sound data: {SDL.GetError()}");
        }

        _fedBytes += length;
    }

    private void _destroyStream()
    {
        if (IsStopped) return;

        AudioDevice.Untrack(this);
        SDL.DestroyAudioStream(_stream);
        _stream = IntPtr.Zero;
    }
}
