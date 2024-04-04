Shader "Gameplay/Shadow"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ShadowColor ("ShadowColor", Color) = (1,1,1,1)
		_Color ("Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "RenderType" = "Transparent" "CanUseSpriteAtlas"="true" "IgnoreProjector"="true"}

		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask Off
        Lighting Off
		ZWrite Off
		ZTest Always
		Cull Off
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#include "UnityCG.cginc"
			#include "ColorSpace.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION; 
				float2 uv : TEXCOORD0;
				float3 worldpos : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(half4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Properties)
            UNITY_INSTANCING_BUFFER_END(Props)
			 
			float4 _ShadowColor;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				float4 offset = UNITY_ACCESS_INSTANCED_PROP(Props, _Properties);
				v.vertex.x += ((o.uv.x < 0.5) && (o.uv.y < 0.5)) * offset.x;
				v.vertex.x += ((o.uv.x < 0.5) && (o.uv.y > 0.5)) * offset.y;
				v.vertex.x += ((o.uv.x > 0.5) && (o.uv.y < 0.5)) * offset.z;
				v.vertex.x += ((o.uv.x > 0.5) && (o.uv.y > 0.5)) * offset.w;

				o.worldpos = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);

			    if (i.worldpos.z > 50 || i.worldpos.z < -100) return 0;
				float4 c = _ShadowColor;
				c *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				c.a *= 0.7;
				return c;
			}
			ENDCG
		}
	}
}
