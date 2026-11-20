using System.Runtime.InteropServices;
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
        var rendererPtr = _ValidateRenderer(renderer, out _device);

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
        var rendererPtr = _ValidateRenderer(renderer, out var device);

        if (!ShaderCross.Init())
        {
            throw new Exception($"Can't init SDL_shadercross: {SDL.GetError()}");
        }

        try
        {
            var shaderHandle = _CompileHLSLHandle(device, hlslSource, entryPoint);
            if (shaderHandle == IntPtr.Zero)
            {
                throw new Exception($"Can't compile HLSL shader: {SDL.GetError()}");
            }

            return new Shader(rendererPtr, device, shaderHandle);
        }
        finally
        {
            ShaderCross.Quit();
        }
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

    private static nint _ValidateRenderer(IRenderer renderer, out nint device)
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

    private static unsafe nint _CompileHLSLHandle(nint device, string hlslSource, string entryPoint)
    {
        nint sourcePtr = Marshal.StringToCoTaskMemUTF8(hlslSource);
        nint entryPtr = Marshal.StringToCoTaskMemUTF8(entryPoint);

        try
        {
            var hlslInfo = new ShaderCross.HLSLInfo
            {
                Source = sourcePtr,
                Entrypoint = entryPtr,
                ShaderStage = ShaderCross.ShaderStage.Fragment
            };

            // 1. Compile HLSL -> SPIR-V to reflect resource counts (samplers & uniform buffers)
            nint spirvPtr = ShaderCross.CompileSPIRVFromHLSL(ref hlslInfo, out nuint spirvSize);
            if (spirvPtr == IntPtr.Zero) return IntPtr.Zero;

            try
            {
                nint metadataPtr = ShaderCross.ReflectGraphicsSPIRV(spirvPtr, spirvSize, 0);
                if (metadataPtr == IntPtr.Zero) return IntPtr.Zero;

                try
                {
                    var metadata = Marshal.PtrToStructure<ShaderCross.GraphicsShaderMetadata>(metadataPtr);
                    var formats = SDL.GetGPUShaderFormats(device);

                    // 2a. Direct3D 12: Compile HLSL directly to DXIL so register spaces match SDL3's root signature
                    if ((formats & SDL.GPUShaderFormat.DXIL) != 0)
                    {
                        nint dxilPtr = ShaderCross.CompileDXILFromHLSL(ref hlslInfo, out nuint dxilSize);
                        if (dxilPtr == IntPtr.Zero) return IntPtr.Zero;

                        try
                        {
                            var createInfo = new SDL.GPUShaderCreateInfo
                            {
                                Code = dxilPtr,
                                CodeSize = dxilSize,
                                Entrypoint = entryPoint,
                                Format = SDL.GPUShaderFormat.DXIL,
                                Stage = SDL.GPUShaderStage.Fragment,
                                NumSamplers = metadata.ResourceInfo.NumSamplers,
                                NumUniformBuffers = metadata.ResourceInfo.NumUniformBuffers
                            };

                            return SDL.CreateGPUShader(device, ref createInfo);
                        }
                        finally
                        {
                            SDL.Free(dxilPtr);
                        }
                    }

                    // 2b. Vulkan / Metal: Transpile from SPIR-V
                    var spirvInfo = new ShaderCross.SPIRVInfo
                    {
                        ByteCode = spirvPtr,
                        ByteCodeSize = spirvSize,
                        Entrypoint = entryPtr,
                        ShaderStage = ShaderCross.ShaderStage.Fragment
                    };

                    return ShaderCross.CompileGraphicsShaderFromSPIRV(device, ref spirvInfo, ref metadata.ResourceInfo, 0);
                }
                finally
                {
                    SDL.Free(metadataPtr);
                }
            }
            finally
            {
                SDL.Free(spirvPtr);
            }
        }
        finally
        {
            Marshal.FreeCoTaskMem(sourcePtr);
            Marshal.FreeCoTaskMem(entryPtr);
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
