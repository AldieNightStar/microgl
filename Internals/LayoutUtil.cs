namespace microgl;

internal static class LayoutUtil
{
    public static float SpanLength(IReadOnlyList<float> sizes, float spacing)
    {
        if (sizes.Count == 0) return 0f;
        return sizes.Sum() + spacing * (sizes.Count - 1);
    }

    public static void GrowAt(List<float> sizes, int index, float size)
    {
        if (index == sizes.Count) sizes.Add(0f);
        sizes[index] = MathF.Max(sizes[index], size);
    }
}
