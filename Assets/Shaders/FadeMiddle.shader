Shader "Sprites/FadeMiddle"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Border("Border", Range(0, 0.5)) = 0.1
		_Range("Range", Range(0, 0.5)) = 0.1
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
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
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
				float2 texcoord  : TEXCOORD0;
			};
			
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			fixed4 _Color;
            float _Range;
            float _Border;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                float y = IN.vertex.y / _ScreenParams.y;
                c.a = y < _Border ? 1 :
					  y > (1 - _Border) ? 1 :
					  y < _Border + _Range ? (1 - (y - _Border) / _Range) :
                      y > (1 - _Border - _Range) ? (y - 1 + _Range + _Border) / _Range :
					  0;
				return c;
			}
		ENDCG
		}
	}
}