Shader "Rokid/RayMaskShader"
{
    Properties
    {
        _Color ("Tint", Color) = (1.000000, 1.000000, 1.000000, 1.000000)
        _Mask ("Mask", float) = 1
        // [ToggleOff]  _IsPress ("Pointer Press", Float) = 0

    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

        Pass
        {

            ZWrite OFF
            Blend SrcAlpha OneMinusSrcColor

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            float _Mask;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET0
            {
                //处理渐变
                if (_Mask < i.uv.x)
                {
                    _Color.a = 0;
                }
                else
                {
                    _Color.a *= (1 - i.uv.x);
                }
                return fixed4(_Color.r * _Color.a, _Color.g * _Color.a, _Color.b * _Color.a, 1);
            }
            ENDCG
        }
    }
}
