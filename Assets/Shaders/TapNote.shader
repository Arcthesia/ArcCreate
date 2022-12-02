Shader "Gameplay/TapNote"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {} 
		_Alpha ("Alpha", Float) = 1
		_Highlight("Highlight", Int) = 0
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "RenderType"="Transparent" "CanUseSpriteAtlas"="true"  }
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "ColorSpace.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			
			int _Highlight; 
			float _Alpha;
			sampler2D _MainTex;
			float4 _Color;

			half4 Highlight(half4 c)
			{
				fixed3 hsv = rgb2hsv(c.rgb);
				hsv.r += 0.4f;
				return half4(hsv2rgb(hsv),c.a);
			}
			 
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{ 
				half4 c = tex2D(_MainTex,i.uv);
				c.a *= _Alpha;
				if(_Highlight == 1) 
				{
					c = Highlight(c);
				}
				return c * _Color;
			}
			ENDCG
		}
	}
}
