Shader "Gameplay/Shadow"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ShadowColor ("ShadowColor", Color) = (1,1,1,1)
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
				uint instanceID : BLENDINDICES0;
			};
			
			struct Properties
			{
				float from;
				float4 color;
			};
			 
			float4 _ShadowColor;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			StructuredBuffer<Properties> _Properties;

			v2f vert (appdata v, uint instanceID : SV_InstanceID)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				o.worldpos = mul(unity_ObjectToWorld, v.vertex);
				v.vertex.y -= o.worldpos.y;
				o.worldpos.y = 0;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.instanceID = instanceID;
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				Properties properties = _Properties[i.instanceID];
			    if(i.uv.y < properties.from || i.worldpos.z > 50 || i.worldpos.z < -100) discard;
				float4 c = _ShadowColor;
				c *= properties.color;
				c.a *= 0.7;
				return c;
			}
			ENDCG
		}
	}
}
