namespace microgl;

public record struct SynthSettings
{
    public SynthSettings() { }

    public const int DefaultSampleRate = 44100;

    public SoundWaveform Waveform { get; init; } = SoundWaveform.Sine;
    public float Duration { get; init; } = 0.5f;
    public int SampleRate { get; init; } = DefaultSampleRate;
    public float Frequency { get; init; } = 440f;
    public float EndFrequency { get; init; } = 440f;
    public float Volume { get; init; } = 0.5f;
    public float Attack { get; init; } = 0.01f;
    public float Decay { get; init; } = 0.1f;
    public float Sustain { get; init; } = 0.7f;
    public float Release { get; init; } = 0.2f;
    public float DutyCycle { get; init; } = 0.5f;
    public int Partials { get; init; } = 3;
    public float Brightness { get; init; } = 0.5f;
}