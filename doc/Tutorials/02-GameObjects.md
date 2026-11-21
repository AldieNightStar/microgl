# Tutorial 2: Game objects and sprites

In this tutorial you build an animated player that follows the mouse and collects coins. Each thing in the game becomes its own `GameObject`, and a `GameObjectGroup` runs them all.

You will use: [GameObject](../GameObject/GameObject.md), [GameObjectGroup](../GameObject/GameObjectGroup.md), [AtlasTexture](../Drawing/AtlasTexture.md), [AnimatedTexture](../Drawing/AnimatedTexture.md), [Mouse](../Input/Mouse.md).

### Step 1: The idea

A `GameObject` has a `Position`, `Rotation`, `Scale` and four methods you fill in:

* `OnInit` runs once, before the first update.
* `OnUpdate` runs every frame. Use `Time.DeltaTime` for the frame time.
* `OnDraw` runs every frame. Draw at `(0, 0)`: the drawer is already moved, rotated and scaled to the object.
* `Dispose` frees what the object owns.

You never call `OnInit`, `OnUpdate` or `OnDraw` yourself. You call `Init`, `Update` and `Draw`, or you put the object in a group that does it for you.

### Step 2: A coin

```csharp
public class Coin : GameObject
{
    private const int SIZE = 16;

    public Coin(Vector2 position)
    {
        Position = position;
        CenterPoint = new Vector2(SIZE / 2f, SIZE / 2f);
    }

    protected override void OnInit(Drawer d) { }

    protected override void OnUpdate(Drawer d) { }

    protected override void OnDraw(Drawer d)
    {
        d.Color = Color.Gold;
        d.DrawRoundedRect(new Rectangle(0, 0, SIZE, SIZE), SIZE / 2);
    }

    public override void Dispose() { }
}
```

`CenterPoint` moves the pivot to the middle of the coin, so `Position` is the coin's center, and rotation and scale happen around it.

### Step 3: An animated player

The sprite sheet `hero_run.png` holds equally sized 32x32 frames in a row. `AtlasTexture.FromTexture` cuts it into frames, and `AnimatedTexture` plays them.

```csharp
public class Player : GameObject
{
    private const float SPEED = 200f;
    private const float FRAME_TIME = 0.1f;
    private const float STOP_DISTANCE = 4f;
    private const int FRAME_SIZE = 32;

    private readonly Texture _sheet;
    private AnimatedTexture _run = null!;

    public Player(Texture sheet, Vector2 position)
    {
        _sheet = sheet;
        Position = position;
        CenterPoint = new Vector2(FRAME_SIZE / 2f, FRAME_SIZE / 2f);
    }

    protected override void OnInit(Drawer d)
    {
        var frames = AtlasTexture.FromTexture(_sheet, new Vector2(FRAME_SIZE, FRAME_SIZE));
        _run = new AnimatedTexture(frames, FRAME_TIME);
    }

    protected override void OnUpdate(Drawer d)
    {
        var toMouse = Mouse.Position - Position;
        if (toMouse.Length() < STOP_DISTANCE) return;

        Position += Vector2.Normalize(toMouse) * SPEED * Time.DeltaTime;
        Scale = new Vector2(toMouse.X < 0f ? -1f : 1f, 1f);
        _run.Update(Time.DeltaTime);
    }

    protected override void OnDraw(Drawer d)
    {
        d.DrawTexture(_run, new Rectangle(0, 0, FRAME_SIZE, FRAME_SIZE));
    }

    public override void Dispose() => _run.Dispose();
}
```

A negative `Scale.X` mirrors the sprite, so the player faces the mouse. Because of `CenterPoint`, it flips in place.

The player does not own the sheet texture, so the game frees it.

### Step 4: Put everything in a group

A `GameObjectGroup` inits, updates, draws and disposes all of its children. Objects you `Add` start on the group's next `Update`. Objects you `Delete()` are disposed and removed on the next `Update`.

```csharp
public class MyGame : IGame
{
    private const int COIN_COUNT = 10;
    private const float PICKUP_DISTANCE = 20f;

    private readonly GameObjectGroup _world = new();
    private readonly List<Coin> _coins = new();
    private Texture _heroSheet = null!;
    private Player _player = null!;
    private int _score;

    public void Init(Drawer d)
    {
        _heroSheet = Texture.FromPNG("Assets/hero_run.png");
        _player = new Player(_heroSheet, d.ScreenSize / 2f);
        _world.Add(_player);

        for (int i = 0; i < COIN_COUNT; i++)
            _world.Add(new Coin(_randomPoint(d)));

        _world.Init(d);
    }

    public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
    {
        _world.Update(d);
        _collectCoins();
    }

    public void Draw(Drawer d, float dt)
    {
        _world.Draw(d);
        d.DrawDebugText($"Coins: {_score}", new Vector2(10f, 10f));
    }

    public void Dispose()
    {
        _world.Dispose();
        _heroSheet.Dispose();
    }

    private void _collectCoins()
    {
        _world.CollectChildren(_coins);
        foreach (var coin in _coins)
        {
            if (!coin.IsAlive || Vector2.Distance(coin.Position, _player.Position) > PICKUP_DISTANCE) continue;
            coin.Delete();
            _score++;
        }
    }

    private static Vector2 _randomPoint(Drawer d)
        => new(Random.Shared.NextSingle() * d.ScreenSize.X, Random.Shared.NextSingle() * d.ScreenSize.Y);
}
```

`CollectChildren` fills a list you keep, so it does not allocate memory every frame.

### Step 5: Move objects together

Children of a group are placed relative to the group. Move, rotate or scale the group and all children follow.

```csharp
var squad = new GameObjectGroup { Position = new Vector2(500f, 100f) };
squad.Add(new Coin(new Vector2(0f, 0f)));
squad.Add(new Coin(new Vector2(24f, 0f)));
squad.Add(new Coin(new Vector2(48f, 0f)));
_world.Add(squad);

squad.Rotation += 0.5f;
```

Use `coin.GlobalPosition` when you need a child's position on screen.

### Next

[Tutorial 3: Buttons and menus](03-ButtonsAndMenus.md) adds a clickable menu.
