namespace microgl;

public static class SoundFX
{
    public static Sound Tone(SynthSettings? settings = null) =>
        _create(settings ?? new SynthSettings());

    public static Sound Square(SynthSettings? settings = null) =>
        _create((settings ?? new SynthSettings()) with { Waveform = SoundWaveform.Square });

    public static Sound Noise(SynthSettings? settings = null) =>
        _create((settings ?? new SynthSettings()) with { Waveform = SoundWaveform.Noise });

    public static Sound Laser(SynthSettings? settings = null) =>
        _create(settings ?? new SynthSettings
        {
            Waveform = SoundWaveform.Square,
            Frequency = 880f,
            EndFrequency = 110f,
            Duration = 0.3f,
            Release = 0.1f,
        });

    public static Sound Coin(SynthSettings? settings = null) =>
        _create(settings ?? new SynthSettings
        {
            Waveform = SoundWaveform.Square,
            Frequency = 988f,
            EndFrequency = 1976f,
            Duration = 0.15f,
            Decay = 0.05f,
            Sustain = 0.5f,
            Release = 0.05f,
        });

    public static Sound Explosion(SynthSettings? settings = null) =>
        _create(settings ?? new SynthSettings
        {
            Waveform = SoundWaveform.Noise,
            Duration = 0.8f,
            Decay = 0.4f,
            Sustain = 0f,
            Release = 0.4f,
        });

    public static Sound Bell(SynthSettings? settings = null)
    {
        var bell = settings ?? new SynthSettings { Frequency = 880f, EndFrequency = 880f, Duration = 1.2f, Release = 0.8f };
        var layers = new List<float[]>();

        for (int partial = 1; partial <= bell.Partials; partial++)
        {
            layers.Add(Synth.Render(bell with
            {
                Frequency = bell.Frequency * partial,
                EndFrequency = bell.EndFrequency * partial,
                Volume = bell.Volume * MathF.Pow(bell.Brightness, partial - 1),
            }));
        }

        return Sound.FromSamples(Synth.Mix(layers), bell.SampleRate);
    }

    private static Sound _create(SynthSettings settings) =>
        Sound.FromSamples(Synth.Render(settings), settings.SampleRate);
}
