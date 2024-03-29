Shader "Unlit/Cube"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Tags { "LightMode" = "ForwardBase" }
        LOD 100
        Zwrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            void frag (v2f i,out fixed4 col : SV_Target, out fixed4 col2 : SV_Target1)
            {
                // sample the texture
                col = tex2D(_MainTex, i.uv);
                col2 = fixed4(1, 0, 0, 1);
            }
            ENDCG
        }
    }
}
