Shader "Gameplay/Waveform"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_HighlightColor ("Highlight color", Color) = (1,1,1,1)
		_Background ("Background", Color) = (1,1,1,1)
		_RepeatColor ("Repeat color", Color) = (1,1,1,1)
		_CurrentTimingSample ("Current sample", Int) = 0
		_AudioLength ("Audio length", Int) = 0
		_RepeatSampleFrom ("Repeat from", Int) = 0
		_RepeatSampleTo ("Repeat to", Int) = 0
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "RenderType" = "Transparent" "CanUseSpriteAtlas"="true" "IgnoreProjector"="true"}

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Off
		Lighting Off
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

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
			
			sampler2D _MainTex;
            float4 _MainTex_TexelSize;
			float4 _Color, _HighlightColor, _Background, _RepeatColor;
            int _CurrentSample, _AudioLength, _RepeatSampleFrom, _RepeatSampleTo;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
            
            float SampleAt(float x)
            {
				// LITERAL DARK MAGIC
				// https://github.com/keijiro/WavTexture/blob/master/Assets/WavTexture/Shader/Common.cginc
				float pos = x / 32 / 4;
				float xy = floor(pos) * _MainTex_TexelSize.x;
				float xcoord = frac(xy);
				float ycoord = floor(xy) * _MainTex_TexelSize.y;
				
				xcoord += _MainTex_TexelSize.x * 0.5;
				ycoord += _MainTex_TexelSize.y * 0.5;
				
				float4 pixel = tex2Dlod(_MainTex, float4(xcoord, ycoord, 0, 0));
				float i = frac(pos);
				float val = i < 0.25 ? pixel.r : i < 0.5 ? pixel.g : i < 0.75 ? pixel.b : pixel.a;

				return val * 2 - 1;
            }
			
			float4 frag (v2f i) : SV_Target
			{ 
				float yFromCenter = abs(i.uv.y - 0.5f) * 2;

				float uvx = i.uv.x;
				float sampleMain = uvx * _AudioLength;
				float dx = ddx(uvx);

				// If it works it's not stupid
				float s = 0;

				[unroll(4)]
				for (int i = 0; i < 4; i++)
				{
					float smpl = (uvx + dx * i / 4) * _AudioLength;
					s += SampleAt(smpl);
				}

				s /= 4;

                bool hit = yFromCenter <= (s + 0.05);
				bool repeat = sampleMain > _RepeatSampleFrom && sampleMain < _RepeatSampleTo;

				if (hit)
				{
					half4 c = sampleMain > _CurrentSample ? _Color : _HighlightColor;
					c = repeat ? lerp(c, _RepeatColor, 0.4) : c;
					return c;
				}
				else
				{
					return _Background;
				}
			}
			ENDCG
		}
	}
}
