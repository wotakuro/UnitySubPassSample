Shader "Unlit/CustomBlit"
{
    Properties
    {
        _Color1("color1", Color) = (.25, .5, .5, 0)
        _Color2("color2", Color) = (.25, .25, .5, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "HLSLSupport.cginc"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color1;
                half4 _Color2;
            CBUFFER_END

            // FrameBufferInputの宣言をします
            UNITY_DECLARE_FRAMEBUFFER_INPUT_FLOAT(0); // Albedo
            UNITY_DECLARE_FRAMEBUFFER_INPUT_FLOAT(1); // Depth

            float4 vert(float4 vertexPosition : POSITION) : SV_POSITION
            {
                return vertexPosition;
            }            
                
            half4 frag(float4 pos : SV_POSITION) : SV_Target
            {

                // Tiledベースアーキテクチャ(Vulkan/metal)では、Textureフェッチではないので早くアクセスが出来ます
                float4 albedo = UNITY_READ_FRAMEBUFFER_INPUT(0, pos);
                float depth = UNITY_READ_FRAMEBUFFER_INPUT(1, pos).r;

                half4 blend = lerp(_Color2, _Color1, depth);
                // depthの値で何となく色味を混ぜるエフェクトです
                albedo.rgb = (albedo.rgb * (1.0 - (blend.a * albedo.a) ) +
                    blend.rgb * blend.a * albedo.a);
                return half4(albedo.rgb,1.0);
            }
            ENDHLSL
        }
    }
}
