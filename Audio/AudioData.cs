using SDL3;

namespace microgl;

internal readonly record struct AudioData(SDL.AudioSpec Spec, nint Buffer, int Length, Action<nint> Release);
