namespace microgl;

public abstract class GameObjectBase : IDisposable
{
    protected abstract void OnInit(Drawer d);
    protected abstract void OnUpdate(Drawer d, float dt);
    protected abstract void OnDraw(Drawer d, float dt);
    public abstract void Dispose();
}