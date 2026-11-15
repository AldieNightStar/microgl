using System.Numerics;
using SDL3;

namespace microgl;

public interface IGame : IDisposable
{
    void Init(Drawer d);
    void Update(Drawer d, ref SDL.Event ev, double dt);
    void Draw(Drawer d, double dt);
}