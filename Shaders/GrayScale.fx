sampler uImage0 : register(s0);

float4 GrayScale(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, coords) * sampleColor;
    float avg = (base.r + base.g + base.b) / 6.0;
    base.rgb = avg.xxx;
    return base;
}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_3_0 GrayScale();
    }
}
