namespace microgl;

public interface IGameUpdatable : IDisposable
{
    void Update(Drawer d);
}