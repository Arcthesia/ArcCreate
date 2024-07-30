// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/BlendModes/VividLight"
{
	Properties
	{
		_Color("Color", Color) = (0, 0, 0, 0) 
		_MainTex("Texture", 2D) = "white" {}
		_Modify("Modify", Vector) = (0, 0, 1, 1)
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" }
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Always

		GrabPass
		{
			"_VividLightGrabTex"
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "BlendModes.cginc"

			float4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _VividLightGrabTex;
			float4 _Modify;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 screen : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screen = ComputeGrabScreenPos(o.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			inline fixed blendVividLight(fixed base, fixed blend)
			{
				return (blend < 0.5) ? blendColorBurn(base, (2.0 * blend)) : blendColorDodge(base, (2.0 * (blend - 0.5)));
			}

			fixed3 blendVividLight(fixed3 base, fixed3 blend)
			{
				return fixed3(
					blendVividLight(base.r, blend.r),
					blendVividLight(base.g, blend.g),
					blendVividLight(base.b, blend.b)
				);
			}

			fixed3 blendVividLight(fixed3 base, fixed3 blend, fixed opacity)
			{
				return (blendVividLight(base, blend) * opacity + base * (1.0 - opacity));
			}

			fixed3 frag(v2f i) : SV_Target
			{
				float4 baseColor = tex2Dproj(_VividLightGrabTex, i.screen);
				i.uv.x += _Modify.x;
				i.uv.x *= _Modify.z;
				i.uv.y += _Modify.y;
				i.uv.y *= _Modify.w;
				float4 texColor = tex2D(_MainTex, i.uv) * _Color;

				return blendVividLight(baseColor, texColor, texColor.a);
			}
			ENDCG
		}
	}
}