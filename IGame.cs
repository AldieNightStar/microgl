using System.Numerics;
using SDL3;

namespace microgl;

public interface IGame : IDisposable
{
    void Init(Drawer d);
    void Update(Drawer d, ReadOnlySpan<SDL.Event> events, float dt);
    void Draw(Drawer d, float dt);
}