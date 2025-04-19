Shader "Rokid/Other/GridShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        _Color ("Color", Color) = (1, 1, 1, 1)

        [Header(WHITEMODEL)]
        [Toggle]_WhiteModel ("White Model On", Float) = 0
        [Toggle(_SCREENSPACE_GRADIENT)]_ScreenspaceGradient ("ScreenspaceGradient", Float) = 0
        _RTIndex ("RTIndex", Int) = 0
        _MaskIndex ("Mask Index", Int) = 0
        _MaskPow ("Mask Power", Float) = 1

        [Enum(UnityEngine.Rendering.Universal.ColorMask)]_ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" }

        ZWrite Off
        ColorMask [_ColorMask]
        Blend SrcAlpha OneMinusSrcAlpha

        CGINCLUDE
        #pragma vertex vert
        #pragma fragment frag
        // make fog work
        #pragma shader_feature _ _SCREENSPACE_GRADIENT
        #include "UnityCG.cginc"


        struct appdata
        {
            float4 positionOS : POSITION;
            float4 uv : TEXCOORD0;
            float4 normal : NORMAL;
        };

        struct v2f
        {
            float4 positionCS : SV_POSITION;
            float4 uv : TEXCOORD0;
            float4 uvSS : TEXCOORD1;
        };


        // TEXTURE2D(_MainTex);
        // SAMPLER(sampler_MainTex);
        sampler2D _MainTex;

        float4 _MainTex_ST;
        half4 _Color;
        half4 _EdgeColor;
        half _MaskPow;
        half _EdgeWidth;
        half _WhiteModel;
        int _MaskIndex;
        int _RTIndex;

        #ifdef _SCREENSPACE_GRADIENT
            // TEXTURE2D(_BackgroundTexture);
            // SAMPLER(sampler_BackgroundTexture);
            // TEXTURE2D(_BackgroundTexture1);
            // SAMPLER(sampler_BackgroundTexture1);
            // TEXTURE2D(_BackgroundTexture2);
            // SAMPLER(sampler_BackgroundTexture2);
            sampler2D _BackgroundTexture;
            sampler2D _BackgroundTexture1;
            sampler2D _BackgroundTexture2;
            
        #endif

        float3x3 FromVector2RotationMatrix(float3 vecFrom, float3 vecTo)
        {
            // 计算两个向量的夹角
            float cosTheta = dot(vecFrom, vecTo);
            float theta = acos(cosTheta);

            // 计算旋转轴的坐标
            float3 axis = cross(vecFrom, vecTo);

            // 构建旋转矩阵
            float cosThetaMinusOne = cosTheta - 1.0;
            float sinTheta = sin(theta);
            float3x3 rotationMatrix = float3x3(
                cosTheta + cosThetaMinusOne * axis.x * axis.x + cosThetaMinusOne * (1.0 - axis.x * axis.x) - sinTheta *
                axis.z,
                cosThetaMinusOne * axis.x * axis.y + sinTheta * axis.z,
                cosThetaMinusOne * axis.x * axis.z - sinTheta * axis.y,
                cosThetaMinusOne * axis.x * axis.y - sinTheta * axis.z,
                cosTheta + cosThetaMinusOne * axis.y * axis.y + cosThetaMinusOne * (1.0 - axis.y * axis.y) - sinTheta *
                axis.x,
                cosThetaMinusOne * axis.y * axis.z + sinTheta * axis.x,
                cosThetaMinusOne * axis.x * axis.z + sinTheta * axis.y,
                cosThetaMinusOne * axis.y * axis.z - sinTheta * axis.x,
                cosTheta + cosThetaMinusOne * axis.z * axis.z + cosThetaMinusOne * (1.0 - axis.z * axis.z) - sinTheta *
                axis.x
            );

            return rotationMatrix;
        }

        v2f vert(appdata v)
        {
            v2f o = (v2f)0;
            // Gradient
            o.uv.z = 1 - v.uv.z;
            // VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
            float4 posWS = mul(UNITY_MATRIX_M, float4(v.positionOS.xyz, 1.0));
            o.positionCS = mul(UNITY_MATRIX_VP, posWS);
            o.uvSS = ComputeScreenPos(o.positionCS);
            float2 uv;
            float3 normalTo = float3(0.0, 1.0, 0.0);
            float3 normalFrom = normalize(mul(v.normal.xyz, (float3x3)unity_WorldToObject));
            float3x3 rotateMatrix = FromVector2RotationMatrix(normalFrom, normalTo);
            uv = mul(rotateMatrix, posWS).xz;
            o.uv.xy = TRANSFORM_TEX(uv, _MainTex);
            return o;
        }

        half4 frag(v2f i) : SV_Target
        {
            half mask = 1;
            #ifdef _SCREENSPACE_GRADIENT
                // #ifdef _WHITEMODEL_ON
                if (_WhiteModel > 0)
                {
                    return 1;
                }
                // #endif

                // sample the texture
                float2 screenPos = i.uvSS.xy / i.uvSS.w;
                half4 maskCol = 0;
                if (_RTIndex == 0)
                {
                    // maskCol = SAMPLE_TEXTURE2D(_BackgroundTexture, sampler_BackgroundTexture, screenPos);
                    maskCol = tex2D(_BackgroundTexture, screenPos);
                }
                else if (_RTIndex == 1)
                {
                    // maskCol = SAMPLE_TEXTURE2D(_BackgroundTexture1, sampler_BackgroundTexture1
                    // , screenPos);
                    maskCol = tex2D(_BackgroundTexture1, screenPos);
                }
                else if (_RTIndex == 2)
                {
                    // maskCol = SAMPLE_TEXTURE2D(_BackgroundTexture2, sampler_BackgroundTexture2, screenPos);
                    maskCol = tex2D(_BackgroundTexture2, screenPos);
                }
                
                mask = maskCol.r;
                if (_MaskIndex == 0)
                {
                    mask = maskCol.r;
                }
                else if (_MaskIndex == 1)
                {
                    mask = maskCol.g;
                }
                else if (_MaskIndex == 2)
                {
                    mask = maskCol.b;
                }
                
            #else

                mask = i.uv.z;

            #endif
            // half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy) * _Color;
            half4 col = tex2D(_MainTex, i.uv.xy) * _Color;
            mask = pow(max(mask, 1e-3), _MaskPow);
            col.a = col.r * mask;
            col.rgb *= _Color.rgb;

            return col;
        }
        ENDCG

        Pass
        {
            Name "Grid-URP"
            //            Tags
            //            { // "LightMode"="UniversalForward" "RenderPipeline" = "UniversalPipeline"
            //            }

            CGPROGRAM
            // #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            ENDCG
        }

        //        Pass
        //        {
        //            Name "Grid-Builtin"
        //
        //            CGPROGRAM
        //
        //            ENDCG
        //        }

    }
}