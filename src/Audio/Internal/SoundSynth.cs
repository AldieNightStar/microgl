namespace microgl;

internal static class SoundSynth
{
    private const float _twoPi = MathF.PI * 2f;

    internal static float[] Render(SynthSettings settings)
    {
        int frames = (int)(settings.Duration * settings.SampleRate);
        var samples = new float[frames];
        float phase = 0f;

        for (int i = 0; i < frames; i++)
        {
            float time = i / (float)settings.SampleRate;
            phase = (phase + _frequencyAt(settings, time) / settings.SampleRate) % 1f;
            samples[i] = _waveSample(settings.Waveform, phase, settings.DutyCycle) * _gainAt(settings, time) * settings.Volume;
        }

        return samples;
    }

    internal static float[] Mix(IReadOnlyList<float[]> layers)
    {
        var mixed = new float[layers[0].Length];
        foreach (var layer in layers)
        {
            for (int i = 0; i < mixed.Length; i++) mixed[i] += layer[i];
        }

        return mixed;
    }

    private static float _frequencyAt(SynthSettings settings, float time)
    {
        float progress = time / settings.Duration;
        return float.Lerp(settings.Frequency, settings.EndFrequency, progress);
    }

    private static float _waveSample(SoundWaveform waveform, float phase, float duty) => waveform switch
    {
        SoundWaveform.Sine => MathF.Sin(_twoPi * phase),
        SoundWaveform.Square => phase < duty ? 1f : -1f,
        SoundWaveform.Triangle => 1f - 4f * MathF.Abs(phase - 0.5f),
        SoundWaveform.Sawtooth => 2f * phase - 1f,
        _ => Random.Shared.NextSingle() * 2f - 1f,
    };

    private static float _gainAt(SynthSettings settings, float time) =>
        _attackGain(settings, time) * _decayGain(settings, time) * _releaseGain(settings, time);

    private static float _attackGain(SynthSettings settings, float time) =>
        settings.Attack > 0f ? MathF.Min(time / settings.Attack, 1f) : 1f;

    private static float _decayGain(SynthSettings settings, float time)
    {
        float progress = settings.Decay > 0f
            ? Math.Clamp((time - settings.Attack) / settings.Decay, 0f, 1f)
            : (time >= settings.Attack ? 1f : 0f);

        return float.Lerp(1f, settings.Sustain, progress);
    }

    private static float _releaseGain(SynthSettings settings, float time) =>
        settings.Release > 0f ? Math.Clamp((settings.Duration - time) / settings.Release, 0f, 1f) : 1f;
}
