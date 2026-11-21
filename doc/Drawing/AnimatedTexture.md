# AnimatedTexture

### Notes
* **Frame animation** over an array of `AtlasTexture` frames, implementing `IGameUpdatable`.
* **All frames must share the same width and height**; the constructor throws for an empty array or mismatched sizes.
* **Advances one frame every `FrameDuration` seconds** when `Update` is called. `Drawer.DrawTexture` always draws `CurrentFrame`.
* **Looping or one-shot**: with `Loop = false` it stops on the last frame and sets `IsFinished`. `OnComplete` fires each time the last frame ends.
* **Does not own the frame textures**: disposing it only stops updates and clears `OnComplete`.

### Usage
```csharp
// Create from equally sized frames, switching every frameDuration seconds
new AnimatedTexture(AtlasTexture[] frames, float frameDuration, bool loop = true);

// Advance using Time.DeltaTime (IGameUpdatable) or an explicit dt
anim.Update(Drawer d);
anim.Update(float dt);

// Playback state
anim.FrameDuration;
anim.Loop;
anim.IsPaused;
anim.IsFinished;
anim.FrameIndex;      // set to jump to a frame (clamped)
anim.FrameCount;
anim.CurrentFrame;
anim.Size;            // size of one frame
anim.OnComplete;

anim.Reset();         // back to frame 0
anim.Dispose();
```

### Example
```csharp
var sheet = Texture.FromPNG("hero_run.png");
var frames = AtlasTexture.FromTexture(sheet, new Vector2(32f, 32f));
var run = new AnimatedTexture(frames, 0.1f);

// In OnUpdate:
run.Update(d);

// In OnDraw:
d.DrawTexture(run, new Rectangle(0, 0, 32, 32));
```
