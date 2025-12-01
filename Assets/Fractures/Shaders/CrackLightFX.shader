Shader "ReV3nus/CrackLightFX"
{
    Properties
    {
        [Header(Base Settings)]

        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _MainTex("Pattern Texture", 2D) = "white"
        _NoiseTex("Noise Texture", 2D) = "white"

        [Header(Animation)]

        _Speed("Flow Speed", Float) = 2.0
        _HeightExtension("Growth Height", Float) = 0.0
        _HeightSteadyStateDistance("the distance from the edge of crack growth sphere to stable animation phase's sphere", Float) = 0.3
        _WiggleFreq("Wiggle Frequency", Float) = 1.0
        _WiggleAmp("Wiggle Amplitude", Float) = 0.0
        _CrackGrowthDistance("Current Crack Growth Distance", Float) = 65536.0

        [Header(Fading)]
        
        _TexColorExpo("Texture Color Exponent", Float) = 1.0
        _TexColorCoeff("Texture Color Multiplication coefficient", Float) = 1.0
        _MaxDistance("Max Fade Distance", Float) = 5.0
        _TopFadeBias("Top Fade Bias", Float) = 0.3
        _DistFadeBias("Distance Fade Bias", Float) = 0.8
        _Softness("Soft Particle Factor", Float) = 1.0
        _TotalFade("Total Fade", Float) = 1.0
    }

    SubShader
    {Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
            "IgnoreProjector" = "True"
        }
        Blend SrcAlpha One 
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS           : POSITION;
                float3 normalOS             : NORMAL;
                float2 uv                   : TEXCOORD0;

            };

            struct Varyings
            {
                float4 positionCS           : SV_POSITION;
                float2 uv                   : TEXCOORD0;
                float4 screenPos            : TEXCOORD1;
            };


            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _MainTex_ST;
                float4 _NoiseTex_ST;
                float _Speed;
                float _HeightExtension;
                float _WiggleFreq;
                float _WiggleAmp;
                float _MaxDistance;
                float _TopFadeBias;
                float _DistFadeBias;
                float _Softness;
                float _TexColorExpo;
                float _TexColorCoeff;
                float _CrackGrowthDistance;
                float _HeightSteadyStateDistance;
                float _TotalFade;
            CBUFFER_END

            sampler2D _MainTex;
            sampler2D _NoiseTex;

            float getNoise(float2 uv)
            {
                return tex2D(_NoiseTex, uv).x;
            }
            float GetBias(float time, float bias)
            {
              return (time / ((((1.0/bias) - 2.0)*(1.0 - time))+1.0));
            }
            float easeOutElastic(float x)
            {
                const float c4 = (2 * 3.14159265357) / 3;

                return x == 0 ? 0
                  : x == 1
                  ? 1
                  : pow(2, -10 * x) * sin((x * 10 - 0.75) * c4) + 1;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 pos = input.positionOS.xyz;
                float3 normal = input.normalOS;
                float2 uv = input.uv;

                float tGrowth = saturate((_CrackGrowthDistance - uv.x) / _HeightSteadyStateDistance);

                pos += normal * uv.y * _HeightExtension * easeOutElastic(tGrowth);

                float wiggle = sin(_Time.y * _WiggleFreq + pos.x) * _WiggleAmp * uv.y;
                pos += normal * wiggle;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(pos);
                output.positionCS = vertexInput.positionCS;
                output.uv = uv;

                output.screenPos = ComputeScreenPos(output.positionCS);

                return output;
            }


            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                if(uv.x > _CrackGrowthDistance){
                    return half4(0,0,0,0);
                    }

                float2 flowUV = float2(uv.x * _MainTex_ST.x - _Time.y * _Speed, uv.y * _MainTex_ST.y);
                half4 texColor = tex2D(_MainTex, flowUV);
                texColor = pow(texColor, _TexColorExpo) * _TexColorCoeff;

                float vertFade = GetBias(saturate(1.0 - uv.y), _TopFadeBias);

                float distFade = GetBias(saturate(1.0 - (uv.x / _MaxDistance)), _DistFadeBias);

                // Depth Fade to avoid hard edges into other objects
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float partDepth = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
                float depthFade = saturate((sceneDepth - partDepth) * _Softness);

                half4 finalColor = _BaseColor * texColor;
                float finalAlpha = finalColor.a * vertFade * distFade * depthFade;
                finalAlpha *= _TotalFade;

                finalColor.rgb *= finalAlpha; 

                return half4(finalColor.rgb, finalAlpha);
            }
            ENDHLSL
        }
    }
}
