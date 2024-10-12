Shader "Gameplay/HoldNote"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
		_Properties ("Properties", Vector) = (0, 0, 0, 0)
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
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(half4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Properties)
            UNITY_INSTANCING_BUFFER_END(Props)
			
			int _Selected;
			sampler2D _MainTex;

			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.worldpos = mul(unity_ObjectToWorld, v.vertex);
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
				UNITY_SETUP_INSTANCE_ID(i);

				float from = UNITY_ACCESS_INSTANCED_PROP(Props, _Properties).y;
				float highlight = UNITY_ACCESS_INSTANCED_PROP(Props, _Properties).z;
			    if(i.uv.y < from || i.worldpos.z > 50 || i.worldpos.z < -100) return 0;
				// i.uv.y = (i.uv.y - 1) * 1 / (1 - from) + 1;
				i.uv.x = (i.uv.x + highlight) / 2;

				half4 c = half4(tex2D(_MainTex,i.uv).rgba); 

				if(UNITY_ACCESS_INSTANCED_PROP(Props, _Properties).x >= 0.5) 
				{
					c = Selected(c);
				};

				return c * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
			}
			ENDCG
		}
	}
}
