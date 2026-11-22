# Time

Frame time and clocks. `Game` updates it for you every frame.

### Notes
* **Static high-resolution frame timer** backed by SDL3 performance counters.
* **Updated automatically** each frame by `Game` via `Time.Tick()`.
* **Clamps `DeltaTime`** to `DeltaTimeMax` (default `0.25f` seconds) to prevent physics or movement spikes during frame drops or window dragging.

### Usage
* `Time.DeltaTime`
  Gets the elapsed time in seconds since the previous frame (clamped to `DeltaTimeMax`).
* `Time.Frame`
  Gets the number of frames since the game started.
* `Time.DeltaTimeMax`
  Gets or sets the maximum allowed frame delta time in seconds (`0.25f` by default).
* `Time.EpochMillis`
  Gets the current wall-clock time in milliseconds since 1970 (UTC). Use it for save dates or anything stored across runs.
* `Time.GetTimeMillis()`
  Returns a high-resolution timestamp in milliseconds. Use it to measure how long something takes.

### Example
```csharp
// Move at the same speed on every computer
position.X += speed * Time.DeltaTime;

// Never move more than 0.1 seconds' worth in one frame, even after a lag spike
Time.DeltaTimeMax = 0.1f;

// Measure how long loading takes
double start = Time.GetTimeMillis();
LoadLevel();
double loadMs = Time.GetTimeMillis() - start;

// Remember when the game was saved
long savedAt = Time.EpochMillis;
```

`IGame.Update` and `IGame.Draw` receive the same value as `dt`. Inside `GameObject.OnUpdate`, use `Time.DeltaTime`.
