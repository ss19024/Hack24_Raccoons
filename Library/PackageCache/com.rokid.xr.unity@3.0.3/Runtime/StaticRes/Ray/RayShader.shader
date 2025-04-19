Shader "Rokid/RayShader"
{
    Properties
    {
        _Color ("Tint", Color) = (1.000000, 1.000000, 1.000000, 1.000000)
        _Length ("RayLength", Float) = 1
        _AlphaPow ("Ray Alpha Pow", Float) = 1
        [ToggleOff]  _IsPress ("Pointer Press", Float) = 0
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
                float4 vertex: POSITION;
                float2 uv: TEXCOORD0;
            };

            struct v2f
            {
                float2 uv: TEXCOORD0;
                float4 vertex: SV_POSITION;
            };

            fixed4 _Color;
            float _Length;
            float _IsPress;
            float _AlphaPow;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i): SV_TARGET0
            {
                //处理虚线逻辑
                if (!_IsPress)
                {
                    int count = _Length / 0.02;
                    int val = ceil(i.uv.x * count) % 2;
                    if (val == 0)
                        _Color.a = 0;
                }
                //处理渐变
                if (i.uv.x < 0.5)
                    _Color.a *= (i.uv.x * 2) * 2;
                if (i.uv.x > 0.5)
                    _Color.a *= (1 - i.uv.x) * 2 * 2;
                _Color.a *= _AlphaPow;
                return fixed4(_Color.r * _Color.a, _Color.g * _Color.a, _Color.b * _Color.a, 1);
            }
            ENDCG

        }
    }
}
