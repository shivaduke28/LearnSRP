Shader "Toy/CubemapPreview"
{
    Properties
    {
        _Cubemap ("Cubemap", Cube) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            Tags
            {
                "LightMode" = "ToyOpaque"
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float3 positionOS : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            UNITY_DECLARE_TEXCUBE(_Cubemap);

            v2f vert(appdata v)
            {
                v2f o;
                // flip y
                o.positionOS = v.vertex.xyz * float3(1, -1, 1);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return UNITY_SAMPLE_TEXCUBE(_Cubemap, i.positionOS);
            }
            ENDCG
        }
    }
}