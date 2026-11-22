using SDL3;

namespace microgl;

internal static class AudioDevice
{
    private const uint _noDevice = 0;

    private static uint _device;
    private static SDL.AudioSpec _spec;
    private static readonly HashSet<Sound> _playing = new();

    internal static uint Device => _device;
    internal static SDL.AudioSpec Spec => _spec;

    internal static void Init()
    {
        _device = SDL.OpenAudioDevice(SDL.AudioDeviceDefaultPlayback, IntPtr.Zero);
        if (_device == _noDevice)
        {
            throw new Exception($"Can't open audio device: {SDL.GetError()}");
        }

        SDL.GetAudioDeviceFormat(_device, out _spec, out _);
    }

    internal static void Track(Sound sound) => _playing.Add(sound);

    internal static void Untrack(Sound sound) => _playing.Remove(sound);

    internal static void Update()
    {
        foreach (var sound in _playing.ToArray()) sound.Update();
    }

    internal static void Shutdown()
    {
        foreach (var sound in _playing.ToArray()) sound.Stop();
        _playing.Clear();
        if (_device == _noDevice) return;

        SDL.CloseAudioDevice(_device);
        _device = _noDevice;
    }
}
