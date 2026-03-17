Shader "Custom/ScreenSpaceFog"
{
    Properties
    {
        _FogTex ("Fog Texture", 2D) = "white" {}
        _GridWidth ("Grid Width", Float) = 20
        _GridHeight ("Grid Height", Float) = 20
        _CellSize ("Cell Size", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }

        Pass
        {
            ZTest Always
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _FogTex;
            float _GridWidth;
            float _GridHeight;
            float _CellSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = v.vertex;
                o.uv  = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Convert screen UV → world position
                float2 worldUV = i.uv;

                float2 gridUV;
                gridUV.x = worldUV.x;
                gridUV.y = worldUV.y;

                float4 fog = tex2D(_FogTex, gridUV);

                return fog;
            }
            ENDHLSL
        }
    }
}