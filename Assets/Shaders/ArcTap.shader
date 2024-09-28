Shader "Gameplay/ArcTap"
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
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite On
		ZTest Less

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
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(half4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Properties)
            UNITY_INSTANCING_BUFFER_END(Props)

			sampler2D _MainTex;
            float4 _MainTex_ST;

			half4 Selected(half4 c)
			{
				fixed3 hsv = rgb2hsv(c.rgb);
				hsv.r += 0.4f;
				return half4(hsv2rgb(hsv),c.a);
			}

			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);

				half4 c = half4(tex2D(_MainTex,i.uv).rgb, 1);
				if(UNITY_ACCESS_INSTANCED_PROP(Props, _Properties).x >= 0.5) 
				{
					c = Selected(c);
				}
				return c * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
			}
			ENDCG
		}
	}
}
