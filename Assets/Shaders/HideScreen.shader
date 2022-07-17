Shader "ImageEffects/HideScreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HideTex ("HideTex", 2D) = "white" {}
		_Value("Value", Range(0, 1)) = 0
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _HideTex;
			float _Value;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float v = 1 - tex2D(_HideTex, i.uv);
				col.rgb = lerp(col.rgb, 0, v < _Value * 1.1);
                return col;
            }
            ENDCG
        }
    }
}
