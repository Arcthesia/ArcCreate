Shader "Particles/Grid"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_GridTex ("Grid Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Always

		Pass
		{
			Blend SrcAlpha One
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float4 screenPos  : TEXCOORD0;
				float2 texcoord : TEXCOORD1;
			};
			
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _GridTex;
			float4 _GridTex_TexelSize;
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.screenPos = ComputeScreenPos(OUT.vertex);
				OUT.color = IN.color * _Color;
				return OUT;
			}

			half4 frag(v2f IN) : SV_Target
			{
				float2 screenCoord = IN.screenPos.xy / IN.screenPos.w;
				screenCoord.y = screenCoord.y * _ScreenParams.y / _GridTex_TexelSize.w / 2 % 1;
				screenCoord.x = screenCoord.x * _ScreenParams.x / _GridTex_TexelSize.z / 2 % 1;
				half4 grid = tex2D(_GridTex, screenCoord);
				half4 mask = tex2D(_MainTex, IN.texcoord);
				half4 c = grid * mask;
				c.rgb = lerp(half4(1,1,1,1), IN.color, c.r);
				return c;
			}
			ENDCG
		}

		Pass
		{
			Blend Zero SrcColor
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float4 screenPos  : TEXCOORD0;
				float2 texcoord : TEXCOORD1;
			};
			
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _GridTex;
			float4 _GridTex_TexelSize;
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.screenPos = ComputeScreenPos(OUT.vertex);
				OUT.color = IN.color * _Color;
				return OUT;
			}

			half4 frag(v2f IN) : SV_Target
			{
				float2 screenCoord = IN.screenPos.xy / IN.screenPos.w;
				screenCoord.y = screenCoord.y * _ScreenParams.y / _GridTex_TexelSize.w / 2 % 1;
				screenCoord.x = screenCoord.x * _ScreenParams.x / _GridTex_TexelSize.z / 2 % 1;
				half4 grid = tex2D(_GridTex, screenCoord);
				half4 mask = tex2D(_MainTex, IN.texcoord);
				half4 c = grid * mask;
				c.rgb = lerp(half4(1,1,1,1), IN.color, c.r);
				return lerp(half4(1,1,1,1), c, c.a);
			}
			ENDCG
		}

	}
}