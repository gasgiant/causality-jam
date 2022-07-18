Shader "Unlit/SpriteShader1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		[HDR]
        _Color ("Tint", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Pass
        {
			Cull Off
			Lighting Off
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float4 color : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Color;
			float _Flash;
			float _TreeMove;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				float3 objPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
				float offset = (objPosition.x) * 1000;
				v.vertex.x += sin(_Time.y + offset) * v.uv.y * _TreeMove;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
                fixed4 col = tex2D(_MainTex, i.uv);
				col *= i.color * _Color;
                return col;
            }
            ENDCG
        }
    }
}
