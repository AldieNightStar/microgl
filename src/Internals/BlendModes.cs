using SDL3;

namespace microgl;

internal static class BlendModes
{
    public static readonly SDL.BlendMode MaskMultiplyMode = SDL.ComposeCustomBlendMode(
        SDL.BlendFactor.Zero, SDL.BlendFactor.SrcAlpha, SDL.BlendOperation.Add,
        SDL.BlendFactor.Zero, SDL.BlendFactor.SrcAlpha, SDL.BlendOperation.Add
    );

    public static readonly SDL.BlendMode PremultipliedBlit = SDL.ComposeCustomBlendMode(
        SDL.BlendFactor.One, SDL.BlendFactor.OneMinusSrcAlpha, SDL.BlendOperation.Add,
        SDL.BlendFactor.One, SDL.BlendFactor.OneMinusSrcAlpha, SDL.BlendOperation.Add
    );
}