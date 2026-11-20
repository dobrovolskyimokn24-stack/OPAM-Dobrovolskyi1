Shader "Lab11/Sobel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Strength ("Strength", Range(0, 1)) = 1
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Strength;

            float lum(float2 uv)
            {
                fixed3 c = tex2D(_MainTex, uv).rgb;
                return dot(c, float3(0.299, 0.587, 0.114));
            }

            fixed4 frag (v2f_img i) : SV_Target
            {
                float2 t = _MainTex_TexelSize.xy;
                float gx = -1 * lum(i.uv + float2(-t.x, -t.y))
                         + -2 * lum(i.uv + float2(-t.x,    0))
                         + -1 * lum(i.uv + float2(-t.x,  t.y))
                         +  1 * lum(i.uv + float2( t.x, -t.y))
                         +  2 * lum(i.uv + float2( t.x,    0))
                         +  1 * lum(i.uv + float2( t.x,  t.y));
                float gy = -1 * lum(i.uv + float2(-t.x, -t.y))
                         + -2 * lum(i.uv + float2(   0, -t.y))
                         + -1 * lum(i.uv + float2( t.x, -t.y))
                         +  1 * lum(i.uv + float2(-t.x,  t.y))
                         +  2 * lum(i.uv + float2(   0,  t.y))
                         +  1 * lum(i.uv + float2( t.x,  t.y));
                float e = sqrt(gx * gx + gy * gy);
                fixed4 src = tex2D(_MainTex, i.uv);
                fixed4 fx = fixed4(e, e, e, 1);
                return lerp(src, fx, _Strength);
            }
            ENDCG
        }
    }
}