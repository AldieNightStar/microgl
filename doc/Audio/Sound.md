# Sound

A WAV sound loaded into memory, with its own playback controls: `Play`, `Pause`, `Stop`, seeking, speed, volume and looping.

### Notes
* **Load once, dispose once.** Load in `Init`, dispose in `Dispose`, never every frame.
* **Sources**: a WAV file, WAV bytes in memory, raw PCM bytes with a format, or float samples (mono, from `SoundFX`).
* **One playback per sound.** `Play` on a playing sound does nothing. `Play(replay: true)` restarts it. To overlap two playbacks, load the sound twice.
* **`Play`** starts from `Position`. After `Pause` it resumes where it stopped. After `Stop` it starts from the beginning.
* **`Pause`** keeps the position. **`Stop`** ends the playback and resets the position to `0`.
* **`Position`** is in seconds. Setting it while stopped sets where the next `Play` starts.
* **`Speed`** changes the playback rate, so it changes pitch too. `1` is normal.
* **`Volume`** is per sound, from `0` (silent) to `1` (full).
* **`Loop`** repeats the sound until `Stop`.
* **Focus loss** is not handled for you: call `Pause` yourself if you want sound to stop when the window loses focus.
* **Disposing a sound** stops it.

### Usage
```csharp
Sound.FromWAV(string filename);
new Sound(string filename);
Sound.FromWAVBytes(byte[] bytes);
new Sound(SDL.AudioSpec spec, byte[] pcm);
Sound.FromSamples(float[] samples, int sampleRate);

sound.Play(bool replay = false);
sound.Pause();
sound.Stop();
sound.Dispose();

sound.Loop;      // get or set
sound.Volume;    // get or set, 0 to 1
sound.Speed;     // get or set, 1 is normal
sound.Position;  // get or set, in seconds
sound.Duration;  // length in seconds

sound.IsPlaying;
sound.IsPaused;
sound.IsStopped;
```

### Example: music and sound effects
```csharp
private Sound _theme = null!;
private Sound _hit = null!;

public void Init(Drawer d)
{
    _theme = Sound.FromWAV("theme.wav");
    _hit = Sound.FromWAV("hit.wav");
    _theme.Loop = true;
    _theme.Volume = 0.5f;
    _theme.Play();
}

public void Draw(Drawer d, float dt)
{
    if (Keyboard.IsKeyDown(SDL.Keycode.Space)) _hit.Play();
}

public void Dispose()
{
    _theme.Dispose();
    _hit.Dispose();
}
```

### Example: pause and seek
```csharp
_theme.Pause();
_theme.Position = 10f;
_theme.Play();
```
