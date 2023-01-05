Shader "olihewi/CrosshairCircle"
{
    Properties
    {
        [MainColor] _Color ("Color", Color) = (1,1,1,1)
        _Width ("Width", Range(0,100)) = 0.1
    }
    SubShader
    {
 		Tags 
 		{ 
 			"RenderType" = "Transparent"
 			"Queue" = "Transparent"
            "IgnoreProjector" = "True"
 		}
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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

            float4 _Color;
            float _Width;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = (v.uv - float2(0.5,0.5)) * 1.05;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float radius = length(i.uv) * 2.0;
                float signedDist = radius - 1.0F;
                float2 gradient = float2(ddx(signedDist),ddy(signedDist));
                float rangeFromLine = abs(signedDist / length(gradient));
                float lineWeight = clamp(_Width - rangeFromLine, 0.0, 1.0);
                return float4(_Color.rgb,lineWeight);
            }
            ENDCG
        }
    }
}
