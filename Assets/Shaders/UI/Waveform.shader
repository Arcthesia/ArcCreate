Shader "Compose/Waveform"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Background ("Background", Color) = (1,1,1,1)
		_FromSample ("FromSample", Int) = 0
		_ToSample ("ToSample", Int) = 0
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "RenderType" = "Transparent" "CanUseSpriteAtlas"="true" "IgnoreProjector"="true"}

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Off

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
			float4 _Color, _Background;
            int _FromSample, _ToSample;

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
				float dx = ddx(uvx);

				// If it works it's not stupid
				float s = 0;

				[unroll(6)]
				for (int i = 0; i < 6; i++)
				{
					float smpl = _FromSample + (uvx + dx * i / 6) * (_ToSample - _FromSample);
					s += SampleAt(smpl);
				}

				s /= 6;

                bool hit = yFromCenter >= s;
				return hit ? _Color : _Background;
			}
			ENDCG
		}
	}
}
