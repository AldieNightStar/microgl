using SDL3;

namespace microgl;

public interface IGame : IDisposable
{
    void Init();
    void Update(ref SDL.Event ev, double dt);
    void Draw(Drawer d, double dt);
}