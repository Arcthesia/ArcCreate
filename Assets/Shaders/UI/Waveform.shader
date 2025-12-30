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
				/* works by storing samples as RGBA image
				 * consider a signed 8-bit sample is in its -127~128 range
				 * it's converted to 0~255 then 0.0f~1.0f into the RGBA image
				 * R = sample[0]
				 * G = sample[1]
				 * B = sample[2]
				 * A = sample[3]
				 * parameter x is divided by 4 and its sample size
				 * the fraction part now indexes the sample on the RGBA channel 
				*/
				const int AverageSampleCount = 32;
				float pos = (x-(x%AverageSampleCount)) / AverageSampleCount / 4;
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
				float yFromCenter = abs(i.uv.y - 0.5f)*2;

				float uvx = i.uv.x;
				float dx = ddx(uvx);

				const float SAMPLING_RANGE =  6;
				// If it works it's not stupid
				float s = 0;
				int totalSamples = _ToSample - _FromSample;
				[unroll(SAMPLING_RANGE)]
				for (int j = 0; j < SAMPLING_RANGE; j++)
				{
					float smpl = _FromSample + (uvx + dx * j / SAMPLING_RANGE) * (_ToSample - _FromSample);
					smpl = smpl - (smpl%(SAMPLING_RANGE));
						s += abs(SampleAt(smpl));
				}
				s/=SAMPLING_RANGE;
				float pos = i.uv.y;
				
				float edge = abs(i.uv.y - 0.5f)/s;
				return lerp(_Color, _Background, clamp(0, 1, pow(edge*2, 4)));
			}
			ENDCG
		}
	}
}
