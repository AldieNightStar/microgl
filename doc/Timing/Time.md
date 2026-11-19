# Time

### Notes
* **Static high-resolution frame timer** backed by SDL3 performance counters.
* **Updated automatically** each frame by `Game` via `Time.Tick()`.
* **Clamps `DeltaTime`** to `DeltaTimeMax` (default `0.25f` seconds) to prevent physics or movement spikes during frame drops or window dragging.

### Usage
* `Time.DeltaTime`
  Gets the elapsed time in seconds since the previous frame (clamped to `DeltaTimeMax`).
* `Time.DeltaTimeMax`
  Gets or sets the maximum allowed frame delta time in seconds (`0.25f` by default).
* `Time.GetTimeMillis()`
  Returns the current high-resolution timestamp in milliseconds.

### Example
```csharp
// Set maximum delta time for lags
Time.DeltaTimeMax = 0.1f;

// Use delta time
position.X += speed * Time.DeltaTime;

// Get time in milliseconds since epoch
long nowMs = Time.EpochMillis;

// Get time by SDL perfomance counter
double nowMs = Time.GetTimeMillis();
```
