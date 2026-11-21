using SDL3;

namespace microgl;

public readonly record struct DirectionKeys(SDL.Keycode Up, SDL.Keycode Left, SDL.Keycode Down, SDL.Keycode Right)
{
    public static readonly DirectionKeys Arrows = new(SDL.Keycode.Up, SDL.Keycode.Left, SDL.Keycode.Down, SDL.Keycode.Right);
    public static readonly DirectionKeys Wasd = new(SDL.Keycode.W, SDL.Keycode.A, SDL.Keycode.S, SDL.Keycode.D);
}
