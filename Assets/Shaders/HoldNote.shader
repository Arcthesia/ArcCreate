Shader "Gameplay/HoldNote"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "RenderType"="Transparent" "CanUseSpriteAtlas"="true"  }
		ZWrite Off
		ZTest Always
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

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
				int selected;
			};
			
			int _Selected;
			sampler2D _MainTex;
			
			StructuredBuffer<Properties> _Properties;

			v2f vert (appdata v, uint instanceID : SV_InstanceID)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.worldpos = mul(unity_ObjectToWorld, v.vertex);
				o.instanceID = instanceID;
				return o;
			}

			half4 Selected(half4 c)
			{
				fixed3 hsv = rgb2hsv(c.rgb);
				hsv.r += 0.4f;
				hsv.b += 0.25f;
				return half4(hsv2rgb(hsv),c.a);
			}
			
			half4 frag (v2f i) : SV_Target
			{
				Properties properties = _Properties[i.instanceID];
			    if(i.uv.y < properties.from || i.worldpos.z > 50 || i.worldpos.z < -100) return 0;
				i.uv.y = (i.uv.y - 1) * 1 / (1 - properties.from) + 1;
				half4 c = half4(tex2D(_MainTex,i.uv).rgb, 1); 
				if(properties.selected == 1) 
				{
					c = Selected(c);
				};
				return c * properties.color;
			}
			ENDCG
		}
	}
}
