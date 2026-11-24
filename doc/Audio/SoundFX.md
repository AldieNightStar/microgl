# SoundFX

Generates sound effects in code, with no files. Every method returns a `Sound` you can play, pause, seek, loop and change like any other.

### Notes
* **Settings records** control everything: waveform, duration, pitch (`Frequency`, and `EndFrequency` for a sweep), volume, and the envelope (`Attack`, `Decay`, `Sustain`, `Release`).
* **Passing `null`** uses defaults: the standard settings for `Tone`, `Square` and `Noise`, and a preset for `Laser`, `Coin`, `Explosion` and `Bell`. Pass your own `SynthSettings` to change them. `Square` and `Noise` always set their own waveform, even when you pass settings.
* **Generated sounds are mono, 44100 Hz by default.**

### Usage
```csharp
SoundFX.Tone(SynthSettings? settings = null);
SoundFX.Square(SynthSettings? settings = null);
SoundFX.Noise(SynthSettings? settings = null);
SoundFX.Laser(SynthSettings? settings = null);
SoundFX.Coin(SynthSettings? settings = null);
SoundFX.Explosion(SynthSettings? settings = null);
SoundFX.Bell(SynthSettings? settings = null);
```

### Settings
* `Waveform`: `Sine`, `Square`, `Triangle`, `Sawtooth` or `Noise`.
* `Duration`: length in seconds.
* `Frequency` and `EndFrequency`: pitch in Hz, sweeping from start to end over the duration.
* `Volume`: `0` to `1`.
* `Attack`, `Decay`, `Sustain`, `Release`: envelope in seconds, with `Sustain` as a level from `0` to `1`.
* `DutyCycle`: width of a `Square` pulse, `0.5` is even.
* `SampleRate`: samples per second.
* `Partials` (number of overtones) and `Brightness` (how quickly overtones get quieter) are used by `Bell`.

Defaults: `Sine`, 0.5 s, 44100 Hz, 440 Hz, volume 0.5, attack 0.01, decay 0.1, sustain 0.7, release 0.2, duty cycle 0.5, 3 partials, brightness 0.5.

### Example: laser and coin
```csharp
_laser = SoundFX.Laser();
_coin = SoundFX.Coin(new SynthSettings { Waveform = SoundWaveform.Triangle, Frequency = 660f, EndFrequency = 990f, Duration = 0.2f });

_laser.Play();
```

### Example: bell
```csharp
var bell = SoundFX.Bell(new SynthSettings { Frequency = 523f, EndFrequency = 523f, Duration = 2f, Partials = 4, Brightness = 0.4f });
bell.Play();
```
