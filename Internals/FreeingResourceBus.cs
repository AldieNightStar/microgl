namespace microgl;

internal class FreeingResourceBus {
    public static event Action<Texture>? FreedTexture;

    public static void EmitFreedTexture(Texture texture)
    {
        if (texture == null) return;
        FreedTexture?.Invoke(texture);
    }

    public static void ClearSubscribers()
    {
        FreedTexture = null;
    }
}