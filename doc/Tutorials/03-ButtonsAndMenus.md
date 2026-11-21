# Tutorial 3: Buttons and menus

In this tutorial you make a button, style it, build a main menu, and switch between the menu and the game.

You will use: [Button](../GameObject/UI/Button.md), [Label](../GameObject/UI/Label.md), [UITable](../GameObject/UI/UITable.md), [GameFont](../Drawing/GameFont.md).

All UI elements are game objects, so they follow the same rule as in [Tutorial 2](02-GameObjects.md): call `Init` once, `Update` and `Draw` every frame, and `Dispose` at the end.

### Step 1: One button

```csharp
public class MyGame : IGame
{
    private const int FONT_SIZE = 20;

    private GameFont _font;
    private Button _button = null!;
    private int _clicks;

    public void Init(Drawer d)
    {
        _font = GameFont.FromTTF("Assets/Days.ttf", FONT_SIZE);
        _button = new Button(_font, d.ScreenSize / 2f, new Vector2(200f, 44f), "Click me");
        _button.OnClick += () => _button.Text = $"Clicked {++_clicks} times";
        _button.Init(d);
    }

    public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt) => _button.Update(d);

    public void Draw(Drawer d, float dt) => _button.Draw(d);

    public void Dispose()
    {
        _button.Dispose();
        _font.Dispose();
    }
}
```

The position is the center of the button, because buttons use `centerOrigin: true` by default. The button grows a little on hover and squishes on click. Text that is too long for the button is cut at its edge.

### Step 2: Style the button

```csharp
_button.Color = Color.SeaGreen;
_button.HoverColor = Color.MediumSeaGreen;
_button.PressedColor = Color.DarkGreen;
_button.BorderColor = Color.White;
_button.TextColor = Color.White;

_button.Icon = Texture.FromPNG("Assets/star.png");
_button.IconSize = new Vector2(24f, 24f);
```

With a `backgroundTexture` the button draws that image instead of a rectangle, and tints it on hover and press:

```csharp
var fancy = new Button(_font, new Vector2(360f, 400f), new Vector2(200f, 50f), "Start",
    backgroundTexture: Texture.FromPNG("Assets/button.png"));
```

Change or turn off the animations:

```csharp
_button.HoverAnimation = btn => TweenCollection.Wiggle(btn);
_button.PressAnimation = null;
```

Set `Enabled = false` to grey out a button and ignore clicks.

### Step 3: A main menu

A [UITable](../GameObject/UI/UITable.md) places elements in rows. `AddRow` puts each element on its own row, so you get a vertical list. Only the table needs `Init`, `Update`, `Draw` and `Dispose`: it runs its children.

```csharp
private static readonly Vector2 BUTTON_SIZE = new(180f, 40f);

private UITable _createMenu(Drawer d)
{
    var play = new Button(_font, Vector2.Zero, BUTTON_SIZE, "Play");
    var options = new Button(_font, Vector2.Zero, BUTTON_SIZE, "Options") { Enabled = false };
    play.OnClick += () => _showMenu(false);

    var menu = new UITable(d.ScreenSize / 2f, centerOrigin: true)
    {
        Padding = 20f,
        Spacing = new Vector2(0f, 10f),
        HorizontalAlign = TextAlign.Center,
        BackgroundColor = Color.FromArgb(200, 30, 35, 48),
        CornerRadius = 12
    };

    menu.AddRow(new Label(_font, Vector2.Zero, "My Game", Color.Gold))
        .AddRow(play)
        .AddRow(options);

    return menu;
}
```

Elements inside a table can use `Vector2.Zero` as position: the table moves them into place.

### Step 4: Switch between menu and game

Set `IsActive = false` to hide an object. A hidden object is not updated or drawn, and its buttons ignore the mouse.

```csharp
private UITable _menu = null!;
private Button _back = null!;

public void Init(Drawer d)
{
    _font = GameFont.FromTTF("Assets/Days.ttf", FONT_SIZE);

    _menu = _createMenu(d);
    _back = new Button(_font, new Vector2(80f, 30f), new Vector2(120f, 36f), "Back");
    _back.OnClick += () => _showMenu(true);

    _menu.Init(d);
    _back.Init(d);
    _showMenu(true);
}

public void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt)
{
    _menu.Update(d);
    _back.Update(d);
}

public void Draw(Drawer d, float dt)
{
    _menu.Draw(d);
    _back.Draw(d);
}

public void Dispose()
{
    _menu.Dispose();
    _back.Dispose();
    _font.Dispose();
}

private void _showMenu(bool visible)
{
    _menu.IsActive = visible;
    _back.IsActive = !visible;
}
```

For bigger games, put each screen in its own `GameObjectGroup` and toggle the groups the same way.

### Next

[Tutorial 4: UI layout with UITable and PagedList](04-UILayout.md) shows grids, forms and paged lists.
