# Shader

A GPU effect for textures, such as grayscale or a hit flash. Set `drawer.Shader`, draw, then set it back to `null`.

## Notes
- Wraps a fragment shader (`SDL_GPUShader`) and its `SDL_GPURenderState` for `GPURenderer`.
- Requires an `IRenderer` backed by an active `SDL_GPUDevice` (`SoftwareRenderer` is not supported).
- Supports precompiled bytecode (`SPIRV`, `DXIL`, `MSL`) or runtime HLSL compilation via `SDL_shadercross`.
- Implements `IDisposable` to release native GPU shader and render state handles.

## Usage
```csharp
// Create a shader from raw compiled bytecode bytes
new Shader(IRenderer renderer, ReadOnlySpan<byte> bytecode, SDL.GPUShaderFormat format, string entryPoint = "main", uint numUniformBuffers = 0, uint numSamplers = 1);

// Load a precompiled shader binary file (.spv, .dxil, .msl)
Shader.FromFile(IRenderer renderer, string path, SDL.GPUShaderFormat format, string entryPoint = "main", uint numUniformBuffers = 0, uint numSamplers = 1);

// Compile an HLSL fragment shader directly from a source string
Shader.FromHLSL(IRenderer renderer, string hlslSource, string entryPoint = "main");

// Load and compile an HLSL fragment shader from a file
Shader.FromHLSLFile(IRenderer renderer, string path, string entryPoint = "main");

// Check if native shader and render state handles are valid and not disposed
shader.IsValid();

// Upload an unmanaged uniform struct to fragment buffer slot (register bN, space3)
shader.SetUniform<T>(uint slotIndex, in T data);

// Release native GPU shader and render state resources
shader.Dispose();
```

## Example
```csharp
// Create from bytecode
byte[] bytes = File.ReadAllBytes("effect.spv");
var shader = new Shader(drawer.Renderer, bytes, SDL.GPUShaderFormat.SPIRV);

// Load precompiled file
var fileShader = Shader.FromFile(drawer.Renderer, "effect.spv", SDL.GPUShaderFormat.SPIRV, numUniformBuffers: 1);

// Compile from HLSL string
var hlslShader = Shader.FromHLSL(drawer.Renderer, hlslCode);

// Compile from HLSL file
var hlslFileShader = Shader.FromHLSLFile(drawer.Renderer, "effect.hlsl");

// Check validity & set uniform
if (hlslFileShader.IsValid())
{
    hlslFileShader.SetUniform(0, new MyParams
    {
        Time = 1.5f
    });
}

// Apply during draw and dispose on cleanup
drawer.Shader = hlslFileShader;
drawer.DrawTexture(texture, rect);
drawer.Shader = null;

hlslFileShader.Dispose();
```

## Example 1
Grayscale effect (no uniforms).

```csharp
string grayscaleHlsl = """
    Texture2D<float4> u_texture : register(t0, space2);
    SamplerState u_sampler : register(s0, space2);

    struct PSInput
    {
        float4 v_color : TEXCOORD0;
        float2 v_uv    : TEXCOORD1;
    };

    float4 main(PSInput input) : SV_Target0
    {
        float4 texColor = u_texture.Sample(u_sampler, input.v_uv) * input.v_color;
        float gray = dot(texColor.rgb, float3(0.299, 0.587, 0.114));
        return float4(gray, gray, gray, texColor.a);
    }
    """;

using var grayShader = Shader.FromHLSL(drawer.Renderer, grayscaleHlsl);

drawer.Shader = grayShader;
drawer.DrawTexture(enemyTexture, enemyRect);
drawer.Shader = null;
```

## Example 2
Hit-flash effect using a `Color` / `float4` uniform buffer.

```csharp
[StructLayout(LayoutKind.Sequential)]
struct FlashParams
{
    public float R;
    public float G;
    public float B;
    public float Strength; // 0.0 = normal, 1.0 = solid flash color
}

string flashHlsl = """
    Texture2D<float4> u_texture : register(t0, space2);
    SamplerState u_sampler : register(s0, space2);

    cbuffer Params : register(b0, space3)
    {
        float4 u_flash;
    };

    struct PSInput
    {
        float4 v_color : TEXCOORD0;
        float2 v_uv    : TEXCOORD1;
    };

    float4 main(PSInput input) : SV_Target0
    {
        float4 texColor = u_texture.Sample(u_sampler, input.v_uv) * input.v_color;
        float3 mixed = lerp(texColor.rgb, u_flash.rgb, u_flash.a);
        return float4(mixed, texColor.a);
    }
    """;

using var flashShader = Shader.FromHLSL(drawer.Renderer, flashHlsl);

flashShader.SetUniform(0, new FlashParams
{
    R = 1.0f,
    G = 1.0f,
    B = 1.0f,
    Strength = 0.8f
});

drawer.Shader = flashShader;
drawer.DrawTexture(playerTexture, playerRect);
drawer.Shader = null;
```

## Example 3
Animated rainbow wave using a time uniform.

```csharp
[StructLayout(LayoutKind.Sequential)]
struct TimeParams
{
    public float Time;
    private float _pad1;
    private float _pad2;
    private float _pad3;
}

string rainbowHlsl = """
    Texture2D<float4> u_texture : register(t0, space2);
    SamplerState u_sampler : register(s0, space2);

    cbuffer Params : register(b0, space3)
    {
        float4 u_params;
    };

    struct PSInput
    {
        float4 v_color : TEXCOORD0;
        float2 v_uv    : TEXCOORD1;
    };

    float4 main(PSInput input) : SV_Target0
    {
        float4 texColor = u_texture.Sample(u_sampler, input.v_uv) * input.v_color;
        float3 rainbow = 0.5 + 0.5 * cos(u_params.x * 4.0 + input.v_uv.xyx * 6.0 + float3(0.0, 2.0, 4.0));
        return float4(rainbow * texColor.rgb, texColor.a);
    }
    """;

using var rainbowShader = Shader.FromHLSL(drawer.Renderer, rainbowHlsl);

rainbowShader.SetUniform(0, new TimeParams
{
    Time = elapsedSeconds
});

drawer.Shader = rainbowShader;
drawer.DrawTexture(logoTexture, logoRect);
drawer.Shader = null;
```
