using System.Runtime.InteropServices;
using SDL3;

namespace microgl;

internal static class AudioLoader
{
    private const int _floatBytes = 4;
    private const int _monoChannels = 1;

    internal static AudioData FromFile(string filename)
    {
        if (!SDL.LoadWAV(filename, out var spec, out var buffer, out uint length))
        {
            throw new Exception($"Sound \"{filename}\" wasn't loaded successfully: {SDL.GetError()}");
        }

        return new AudioData(spec, buffer, (int)length, SDL.Free);
    }

    internal static AudioData FromPcm(SDL.AudioSpec spec, byte[] pcm)
    {
        var buffer = Marshal.AllocHGlobal(pcm.Length);
        Marshal.Copy(pcm, 0, buffer, pcm.Length);
        return new AudioData(spec, buffer, pcm.Length, Marshal.FreeHGlobal);
    }

    internal static AudioData FromWavBytes(byte[] bytes)
    {
        var pin = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            var io = SDL.IOFromConstMem(pin.AddrOfPinnedObject(), (nuint)bytes.Length);
            if (!SDL.LoadWAVIO(io, true, out var spec, out var buffer, out uint length))
            {
                throw new Exception($"WAV bytes weren't loaded successfully: {SDL.GetError()}");
            }

            return new AudioData(spec, buffer, (int)length, SDL.Free);
        }
        finally
        {
            pin.Free();
        }
    }

    internal static AudioData FromSamples(float[] samples, int sampleRate)
    {
        var spec = new SDL.AudioSpec
        {
            Format = SDL.AudioFormat.AudioF32LE,
            Channels = _monoChannels,
            Freq = sampleRate,
        };

        var bytes = new byte[samples.Length * _floatBytes];
        Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);
        return FromPcm(spec, bytes);
    }
}
