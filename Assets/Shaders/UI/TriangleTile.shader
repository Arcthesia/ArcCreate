Shader "UI/TriangleTile"
{
	Properties
	{
		_FromColor1 ("From Color 1", Color) = (1,1,1,1)
		_FromColor2 ("From Color 2", Color) = (1,1,1,1)
		_ToColor1 ("To Color 1", Color) = (1,1,1,1)
		_ToColor2 ("To Color 2", Color) = (1,1,1,1)
		_Progress ("Progress", Float) = 1
		_Scale ("Scale", Float) = 1
		_Falloff ("Falloff", Float) = 1
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
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 screenPos : TEXCOORD0;
			};
			
			half4 _FromColor1, _FromColor2, _ToColor1, _ToColor2;
            float _Progress, _Scale, _Falloff;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{ 
                const float sqrt3 = 1.73205080757;
                float2 screenPos = float2((i.screenPos.x - 0.5) * 720 * _ScreenParams.x / _ScreenParams.y, (i.screenPos.y - 0.5) * 720);
                float edgeLength = _Scale;

                // a, b, c are coordinates on the 3 axises on the triangular tile
                float a = ceil((screenPos.x - (sqrt3 / 3 * screenPos.y)) / edgeLength);
                float b = floor(sqrt3 * 2 / 3 * screenPos.y / edgeLength) + 1;
                float c = ceil((-screenPos.x - (sqrt3 / 3 * screenPos.y)) / edgeLength);

                bool isPointingUp = frac((a + b + c) / 2) == 0;
                float triDist = clamp((abs(a) + abs(b) + abs(c)) / _Falloff, 0, 1);

                half4 from = isPointingUp ? _FromColor1 : lerp(_FromColor2, _FromColor1, triDist);
                half4 to = isPointingUp ? _ToColor1 : lerp(_ToColor2, _ToColor1, triDist);
                float blend = clamp(_Progress - triDist / _Falloff, 0, 1);
				return lerp(from, to, blend);
			}
			ENDCG
		}
	}
}
