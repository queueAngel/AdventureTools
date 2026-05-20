sampler uImage0 : register(s0);
float3 uColor;
float2 uImageSize0;

float4 Outline(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 center = tex2D(uImage0, coords) * sampleColor;
    if (center.a > 0)
        return center;

    float2 uvPx = float2(2 / uImageSize0.x, 2 / uImageSize0.y);
    
    float ortho = 0;
    
    // tm
    ortho += tex2D(uImage0, float2(coords.x, coords.y - uvPx.y)).a;
    // ml
    ortho += tex2D(uImage0, float2(coords.x - uvPx.x, coords.y)).a;
    // mr
    ortho += tex2D(uImage0, float2(coords.x + uvPx.x, coords.y)).a;
    // bm
    ortho += tex2D(uImage0, float2(coords.x, coords.y + uvPx.y)).a;
    
    if (ortho != 0)
        return float4(uColor, 1);
    return center;
}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_2_0 Outline();
    }
}