Shader "Gameplay/TapNote"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {} 
		_Color ("Color", Color) = (1,1,1,1)
		_Alpha ("Alpha", Float) = 1
		_Selected("Selected", Int) = 0
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "RenderType"="Transparent" "CanUseSpriteAtlas"="true"  }
		ZWrite On
		ZTest Always
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
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};
			
			int _Selected; 
			float _Alpha;
			sampler2D _MainTex;
			float4 _Color;

			half4 Selected(half4 c)
			{
				fixed3 hsv = rgb2hsv(c.rgb);
				hsv.r += 0.4f;
				return half4(hsv2rgb(hsv),c.a);
			}
			 
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color * _Color;
				o.uv = v.uv;
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{ 
				half4 c = tex2D(_MainTex,i.uv);
				c.a *= _Alpha;
				if(_Selected == 1) 
				{
					c = Selected(c);
				}
				return c * i.color;
			}
			ENDCG
		}
	}
}
