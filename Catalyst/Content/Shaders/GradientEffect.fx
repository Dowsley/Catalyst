#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4 ColorA;
float4 ColorB;

// Sampler for the SpriteTexture, even if not directly used for the gradient,
// SpriteBatch expects it.
Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    // input.TextureCoordinates.y goes from 0 (top) to 1 (bottom)
    // Lerp (Linear Interpolate) between ColorA and ColorB based on the Y coordinate
    float4 gradientColor = lerp(ColorA, ColorB, input.TextureCoordinates.y);
    return gradientColor * input.Color; // Multiply by input.Color to allow tinting from SpriteBatch.Draw
}

technique BasicGradient
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}; 