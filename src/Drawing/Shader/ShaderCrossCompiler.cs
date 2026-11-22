using System.Runtime.InteropServices;
using SDL3;

internal static class ShaderCrossCompiler
{
    public static unsafe nint CompileHLSL(nint device, string hlslSource, string entryPoint)
    {
        if (!ShaderCross.Init())
        {
            throw new Exception($"Can't init SDL_shadercross: {SDL.GetError()}");
        }

        try
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
                if (spirvPtr == IntPtr.Zero)
                    throw new Exception($"Can't compile HLSL to SPIR-V: {SDL.GetError()}");

                try
                {
                    nint metadataPtr = ShaderCross.ReflectGraphicsSPIRV(spirvPtr, spirvSize, 0);
                    if (metadataPtr == IntPtr.Zero)
                        throw new Exception($"Can't reflect SPIR-V metadata: {SDL.GetError()}");

                    try
                    {
                        var metadata = *(ShaderCross.GraphicsShaderMetadata*)metadataPtr;
                        var formats = SDL.GetGPUShaderFormats(device);
                        nint shaderHandle;

                        // 2a. Direct3D 12: Compile HLSL directly to DXIL so register spaces match SDL3's root signature
                        if ((formats & SDL.GPUShaderFormat.DXIL) != 0)
                        {
                            nint dxilPtr = ShaderCross.CompileDXILFromHLSL(ref hlslInfo, out nuint dxilSize);
                            if (dxilPtr == IntPtr.Zero)
                                throw new Exception($"Can't compile HLSL to DXIL: {SDL.GetError()}");

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

                                shaderHandle = SDL.CreateGPUShader(device, ref createInfo);
                            }
                            finally
                            {
                                SDL.Free(dxilPtr);
                            }
                        }
                        else
                        {
                            // 2b. Vulkan / Metal: Transpile from SPIR-V
                            var spirvInfo = new ShaderCross.SPIRVInfo
                            {
                                ByteCode = spirvPtr,
                                ByteCodeSize = spirvSize,
                                Entrypoint = entryPtr,
                                ShaderStage = ShaderCross.ShaderStage.Fragment
                            };

                            shaderHandle = ShaderCross.CompileGraphicsShaderFromSPIRV(
                                device, ref spirvInfo, ref metadata.ResourceInfo, 0);
                        }

                        if (shaderHandle == IntPtr.Zero)
                            throw new Exception($"Can't compile HLSL shader: {SDL.GetError()}");

                        return shaderHandle;
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
        finally
        {
            ShaderCross.Quit();
        }
    }
}
