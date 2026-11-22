namespace microgl;

internal static class FreeingResourceBus<T> where T : class
{
    internal static event Action<T>? Freeing;

    internal static void Emit(T resource) => Freeing?.Invoke(resource);
}
