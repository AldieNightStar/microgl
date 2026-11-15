namespace microgl;

public readonly struct Texture : IDisposable
{
    internal nint Resource {get; init; }
    
    public Texture(nint resource)
    {
        Resource = resource;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}