Shader "Sprites/Masked"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_MaskTex ("Mask Texture", 2D) = "white" {}
		_Modify("Modify", Vector) = (0, 0, 1, 1)
		_Cutoff("Cutoff", Float) = 0
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
				float2 maskTexcoord  : TEXCOORD1;
			};
			
			sampler2D _MainTex, _MaskTex;
            float4 _MainTex_TexelSize, _MaskTex_TexelSize;
			fixed4 _Color;
            float _Cutoff;
            float4 _Modify;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				IN.texcoord.x += _Modify.x;
				IN.texcoord.x *= _Modify.z;
				IN.texcoord.x %= 1;
				IN.texcoord.y += _Modify.y;
				IN.texcoord.y *= _Modify.w;
				IN.texcoord.y %= 1;
                OUT.maskTexcoord = IN.texcoord * _MainTex_TexelSize.xy / _MainTex_TexelSize.xy;
				OUT.color = IN.color * _Color;
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                c.a *= tex2D(_MaskTex, IN.maskTexcoord).a;
				c.a = c.a < _Cutoff ? 0 : c.a;
				return c;
			}
		ENDCG
		}
	}
}