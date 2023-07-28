Shader "Gameplay/SingleLine"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Offset ("Offset", Float) = 0
		_Color ("Color", Color) = (1,1,1,1)
		_Modify("Modify", Vector) = (0, 0, 1, 1)
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "RenderType"="Transparent" "CanUseSpriteAtlas"="true"  }
		ZWrite Off
		ZTest Always
		Cull Off
		Blend SrcAlpha One

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			float _Offset;
			sampler2D _MainTex;
			float4 _Color, _Modify;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color * _Color;
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			    float2 p = i.uv;
				p.x += _Modify.x;
				p.x *= _Modify.z;
				p.x %= 1;
				p.y += _Modify.y;
				p.y *= _Modify.w;
				p.y %= 1;
				p.y += _Offset; 
				float4 c= tex2D(_MainTex,p) * i.color;
				c.a *= 0.75;
				return c;
			}
			ENDCG
		}
	}
}
