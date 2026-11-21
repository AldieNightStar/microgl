using SDL3;

namespace microgl;

public sealed class Shader : IDisposable
{
    private nint _device;
    private nint _shaderHandle;
    internal nint StateHandle { get; private set; }
    private bool _disposed;

    public Shader(
        IRenderer renderer,
        ReadOnlySpan<byte> bytecode,
        SDL.GPUShaderFormat format,
        string entryPoint = "main",
        uint numUniformBuffers = 0,
        uint numSamplers = 1)
    {
        var rendererPtr = _validateRenderer(renderer, out _device);

        _shaderHandle = _CreateShaderHandle(_device, bytecode, format, entryPoint, numUniformBuffers, numSamplers);
        if (_shaderHandle == IntPtr.Zero)
        {
            throw new Exception($"Can't create GPU shader: {SDL.GetError()}");
        }

        StateHandle = _CreateRenderState(rendererPtr, _device, _shaderHandle);
    }

    private Shader(nint rendererPtr, nint device, nint shaderHandle)
    {
        _device = device;
        _shaderHandle = shaderHandle;
        StateHandle = _CreateRenderState(rendererPtr, _device, _shaderHandle);
    }

    public static Shader FromFile(
        IRenderer renderer,
        string path,
        SDL.GPUShaderFormat format,
        string entryPoint = "main",
        uint numUniformBuffers = 0,
        uint numSamplers = 1)
    {
        var bytes = File.ReadAllBytes(path);
        return new Shader(renderer, bytes, format, entryPoint, numUniformBuffers, numSamplers);
    }

    public static Shader FromHLSL(
        IRenderer renderer,
        string hlslSource,
        string entryPoint = "main")
    {
        var rendererPtr = _validateRenderer(renderer, out var device);
        var shaderHandle = ShaderCrossCompiler.CompileHLSL(device, hlslSource, entryPoint);
        return new Shader(rendererPtr, device, shaderHandle);
    }

    public static Shader FromHLSLFile(
        IRenderer renderer,
        string path,
        string entryPoint = "main")
    {
        var hlslSource = File.ReadAllText(path);
        return FromHLSL(renderer, hlslSource, entryPoint);
    }

    public bool IsValid()
    {
        return !_disposed && _shaderHandle != IntPtr.Zero && StateHandle != IntPtr.Zero;
    }

    public void SetUniform<T>(uint slotIndex, in T data) where T : unmanaged
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_SetFragmentUniforms(StateHandle, slotIndex, in data))
        {
            throw new Exception($"Can't set fragment uniform at slot {slotIndex}: {SDL.GetError()}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (StateHandle != IntPtr.Zero)
        {
            SDL.DestroyGPURenderState(StateHandle);
            StateHandle = IntPtr.Zero;
        }

        if (_shaderHandle != IntPtr.Zero && _device != IntPtr.Zero)
        {
            SDL.ReleaseGPUShader(_device, _shaderHandle);
            _shaderHandle = IntPtr.Zero;
        }
    }

    private static nint _validateRenderer(IRenderer renderer, out nint device)
    {
        var rendererPtr = renderer.GetNativePointer();
        if (rendererPtr == IntPtr.Zero)
        {
            throw new ArgumentException("Renderer is not initialized.", nameof(renderer));
        }

        device = SDL.GetGPURendererDevice(rendererPtr);
        if (device == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Renderer is not backed by an SDL_GPUDevice: {SDL.GetError()}");
        }

        return rendererPtr;
    }

    private static nint _CreateRenderState(nint rendererPtr, nint device, nint shaderHandle)
    {
        var stateInfo = new SDL.GPURenderStateCreateInfo
        {
            FragmentShader = shaderHandle
        };

        var stateHandle = SDL.CreateGPURenderState(rendererPtr, ref stateInfo);
        if (stateHandle == IntPtr.Zero)
        {
            SDL.ReleaseGPUShader(device, shaderHandle);
            throw new Exception($"Can't create GPU render state: {SDL.GetError()}");
        }

        return stateHandle;
    }

    private static unsafe nint _CreateShaderHandle(
        nint device,
        ReadOnlySpan<byte> bytecode,
        SDL.GPUShaderFormat format,
        string entryPoint,
        uint numUniformBuffers,
        uint numSamplers)
    {
        fixed (byte* codePtr = bytecode)
        {
            var shaderInfo = new SDL.GPUShaderCreateInfo
            {
                Code = (nint)codePtr,
                CodeSize = (nuint)bytecode.Length,
                Entrypoint = entryPoint,
                Format = format,
                Stage = SDL.GPUShaderStage.Fragment,
                NumSamplers = numSamplers,
                NumUniformBuffers = numUniformBuffers
            };

            return SDL.CreateGPUShader(device, ref shaderInfo);
        }
    }

    private static unsafe bool _SetFragmentUniforms<T>(nint stateHandle, uint slotIndex, in T data) where T : unmanaged
    {
        fixed (T* ptr = &data)
        {
            return SDL.SetGPURenderStateFragmentUniforms(stateHandle, slotIndex, (nint)ptr, (uint)sizeof(T));
        }
    }
}