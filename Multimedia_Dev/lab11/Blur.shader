Shader "Lab11/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Strength ("Strength", Range(0, 1)) = 1
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        float _Strength;

        static const float W[9] = {
            0.01898, 0.05590, 0.12092, 0.19212, 0.22417,
            0.19212, 0.12092, 0.05590, 0.01898
        };

        fixed4 blur(float2 uv, float2 dir)
        {
            fixed4 sum = 0;
            for (int k = -4; k <= 4; k++)
            {
                sum += W[k + 4] * tex2D(_MainTex, uv + dir * k);
            }
            return sum;
        }
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment fragH

            fixed4 fragH (v2f_img i) : SV_Target
            {
                return blur(i.uv, float2(_MainTex_TexelSize.x, 0));
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment fragV

            fixed4 fragV (v2f_img i) : SV_Target
            {
                fixed4 src = tex2D(_MainTex, i.uv);
                fixed4 fx = blur(i.uv, float2(0, _MainTex_TexelSize.y));
                return lerp(src, fx, _Strength);
            }
            ENDCG
        }
    }
}