Shader "Gameplay/Arc"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
		_Properties ("Properties", Vector) = (0, 0, 0, 0)
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "RenderType" = "Transparent" "CanUseSpriteAtlas"="true" "IgnoreProjector"="true"}

		Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
		Cull Off
		ZWrite Off
		ZTest Always
		
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
			
			sampler2D _MainTex;
            float4 _MainTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.worldpos = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			half4 Highlight(half4 c)
			{
				fixed3 hsv = rgb2hsv(c.rgb);
				if(c.r<0.5) hsv.r += 0.1f;
				else if(hsv.g < 1) hsv.g += 1.2f;
				else hsv.b -= 0.2f;
				return half4(hsv2rgb(hsv),c.a);
			}

			half4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);

			    if(i.worldpos.z > 50 || i.worldpos.z < -100) return 0;

				float highlight = UNITY_ACCESS_INSTANCED_PROP(Props, _Properties).y;
				i.uv.x = (i.uv.x + highlight) / 2;
				half4 c = tex2D(_MainTex, i.uv); 
				half4 arcColor = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

				if(UNITY_ACCESS_INSTANCED_PROP(Props, _Properties).x >= 0.5) 
				{
					arcColor = Highlight(arcColor);
				}

				c *= arcColor;
				c.a *= 0.9;
				return c;
			}
			ENDCG
		}
	}
}
