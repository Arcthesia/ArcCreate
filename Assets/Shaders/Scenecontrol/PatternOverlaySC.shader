// Based on Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// 0thElement: Do not question this shader. i don't even know why it works myself
Shader "Sprites/PatternOverlaySC"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)

		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_Modify("Modify", Vector) = (0, 0, 1, 1)

        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _PatternOffset ("Pattern Offset", Vector) = (0, 0, 0, 0)
        _HSVOffset ("HSV Offset", Vector) = (0, 0, 0, 0)
        _Scale ("Scale", Float) = 1
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
            #include "../ColorSpace.cginc"
			
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
				float4 screenPos  : TEXCOORD1;
			};

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;
			float4 _Modify;

			float4 _MainTex_TexelSize;
			sampler2D _OverlayTex;
			float4 _OverlayTex_TexelSize;

            fixed4 _Color;
            float _Scale;
            fixed4 _PatternOffset;
            fixed4 _HSVOffset;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
			
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
                OUT.screenPos = ComputeScreenPos(OUT.vertex);
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

            half4 grabPattern(float4 screenPos, float layer)
            {
                layer = layer + 1;
                screenPos.y = -screenPos.y - layer * 0.2;
				float2 screenCoord = (screenPos.xy / screenPos.w + _PatternOffset * layer * _Time.x / 100) / _Scale * layer;
				screenCoord.y = frac(screenCoord.y * 720 / _OverlayTex_TexelSize.w / 2);
				screenCoord.x = frac(screenCoord.x * 720 * _ScreenParams.x / _ScreenParams.y / _OverlayTex_TexelSize.z / 2);
				return tex2D(_OverlayTex, screenCoord);
            }

			fixed4 SampleSpriteTexture (float2 uv)
			{
				uv.x += _Modify.x;
				uv.x *= _Modify.z;
				uv.y += _Modify.y;
				uv.y *= _Modify.w;
				fixed4 color = tex2D (_MainTex, uv);

#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;
#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
                half4 color = (SampleSpriteTexture(IN.texcoord) + _TextureSampleAdd) * IN.color;
				half4 pattern0 = grabPattern(IN.screenPos, 0);
				half4 pattern1 = grabPattern(IN.screenPos, 0.7);
                half pattern = clamp(pattern0 + pattern1, 0, 1);

                fixed3 hsv = rgb2hsv(color.rgb);
                hsv += _HSVOffset.rgb;
                half4 shifted = half4(hsv2rgb(hsv), color.a);

                color = lerp(color, shifted, pattern);

                return color;
			}
		ENDCG
		}
	}
}