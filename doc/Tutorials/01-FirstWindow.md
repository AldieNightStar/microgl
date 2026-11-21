# Tutorial 1: First window and drawing

In this tutorial you open a window, draw shapes, text and an image, and move a square with the keyboard.

You will use: [Game](../Game.md), [IGame](../IGame.md), [Drawer](../Drawing/Drawer.md), [GameFont](../Drawing/GameFont.md), [Texture](../Drawing/Texture.md), [Keyboard](../Input/Keyboard.md), [Time](../Timing/Time.md).

### Step 1: Create a project

Create a console app next to the `microgl` folder, reference the library, and install the native SDL3 packages:

```bash
dotnet new console -n MyGame
cd MyGame
dotnet add reference ../microgl/microgl.csproj
dotnet add package SDL3-CS.Windows
dotnet add package SDL3-CS.Windows.TTF
dotnet add package SDL3-CS.Windows.Shadercross
```

Assets such as fonts and images are loaded from the working folder, so copy them to the build output in `MyGame.csproj`:

```xml
<ItemGroup>
  <None Include="Assets/**" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

### Step 2: Open an empty window

A game is a class that implements `IGame`. `Game` creates the window and calls your methods every frame:

* `Init` once at start, to load things.
* `Update` every frame, to change things.
* `Draw` every frame, right after the screen is cleared.
* `Dispose` once at the end, to free things.

```csharp
using System.Drawing;
using System.Numerics;
using SDL3;
using microgl;

public class MyGame : IGame
{
    private const int SCREEN_WIDTH = 720;
    private const int SCREEN_HEIGHT = 480;

    public static void Main()
    {
        var game = new Game("My Game", SCREEN_WIDTH, SCREEN_HEIGHT, new MyGame());
        game.Start();
    }

    public void Init(Drawer d) { }

    public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt) { }

    public void Draw(Drawer d, float dt) { }

    public void Dispose() { }
}
```

Run it with `dotnet run`. You get a black 720x480 window. When you resize it, the picture scales and keeps its shape, so you can always draw as if the screen were 720x480.

Coordinates start at the top-left corner `(0, 0)`. `X` grows to the right and `Y` grows down.

### Step 3: Draw shapes and text

Set `d.Color` first, then draw. The color stays until you change it.

```csharp
public void Draw(Drawer d, float dt)
{
    d.Color = Color.CornflowerBlue;
    d.DrawRect(new Rectangle(40, 40, 120, 80));

    d.Color = Color.Orange;
    d.DrawRoundedRect(new Rectangle(200, 40, 120, 80), 16);

    d.Color = Color.White;
    d.DrawLine(new Vector2(40f, 160f), new Vector2(320f, 160f));

    d.DrawDebugText("Debug text needs no font", new Vector2(40f, 180f));
}
```

For real text, load a TrueType font once in `Init` and free it in `Dispose`:

```csharp
private GameFont _font;

public void Init(Drawer d)
{
    _font = GameFont.FromTTF("Assets/Days.ttf", 24);
}

public void Draw(Drawer d, float dt)
{
    d.Font = _font;
    d.FontColor = Color.Gold;
    d.DrawText("Hello, microgl!", new Vector2(40f, 220f));
}

public void Dispose()
{
    _font.Dispose();
}
```

For text that you show every frame, a [Label](../GameObject/UI/Label.md) is faster, because it renders the text once and reuses it.

### Step 4: Move a square with the keyboard

`Keyboard` already knows which keys are held. `GetVectorFromKeys` turns four keys into a direction, then move by `speed * dt`, so the speed is the same on every computer. Use `DirectionKeys.Wasd` for WASD instead.

```csharp
private const float SPEED = 240f;
private const int PLAYER_SIZE = 32;

private Vector2 _player = new(100f, 300f);

public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
{
    var direction = Keyboard.GetVectorFromKeys(DirectionKeys.Arrows);
    _player += direction * SPEED * dt;
}

public void Draw(Drawer d, float dt)
{
    d.Color = Color.LimeGreen;
    d.DrawRect(new Rectangle((int)_player.X, (int)_player.Y, PLAYER_SIZE, PLAYER_SIZE));
}
```

### Step 5: Draw an image

Load images once in `Init`, draw them in `Draw`, and free them in `Dispose`. `BlendColor` tints the image, and white means no tint.

```csharp
private Texture _logo = null!;

public void Init(Drawer d)
{
    _logo = Texture.FromPNG("Assets/logo.png");
}

public void Draw(Drawer d, float dt)
{
    d.DrawTexture(_logo, new Rectangle(400, 200, 128, 128));

    d.BlendColor = Color.Red;
    d.DrawTexture(_logo, new Rectangle(560, 200, 128, 128));
    d.BlendColor = Color.White;
}

public void Dispose()
{
    _logo.Dispose();
}
```

### Next

[Tutorial 2: Game objects and sprites](02-GameObjects.md) shows how to move this code into reusable objects.
