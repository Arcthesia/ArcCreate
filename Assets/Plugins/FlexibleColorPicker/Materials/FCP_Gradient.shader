Shader "Custom/FCP_Gradient" {
	Properties{
		[PerRendererData][Enum(Horizontal,0,Vertical,1,Double,2,HueHorizontal,3,HueVertical,4)] _Mode("Color mode", Int) = 0
		[PerRendererData]_Color1("Color 1", Color) = (1,1,1,1)
		[PerRendererData]_Color2("Color 2", Color) = (1,1,1,1)
		[PerRendererData][Enum(HS, 0, HV, 1, SH, 2, SV, 3, VH, 4, VS, 5)] _DoubleMode("Double mode", Int) = 0
		[PerRendererData]_HSV("Complementing HSV values", Vector) = (0,0,0,1.0)
		[PerRendererData]_HSV_MIN("Min Range value for HSV", Vector) = (0.0,0.0,0.0,0.0)
		[PerRendererData]_HSV_MAX("Max Range value for HSV", Vector) = (1.0,1.0,1.0,1.0)
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
	}
		

	SubShader{
		Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"}

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}

			CGPROGRAM
			#pragma vertex vert  
			#pragma fragment frag
			#include "UnityCG.cginc"

			fixed4 _Color1, _Color2;
			fixed4 _HSV, _HSV_MIN, _HSV_MAX;
			int _Mode, _DoubleMode;

			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 col : COLOR;
			};

			inline fixed4 HSV() {
				float h = abs(_HSV.x) * 6.0;
				float c = _HSV.y * _HSV.z;
				float m = _HSV.z - c;
				float x = c * (1.0 - abs(h % 2.0 - 1.0)) + m;
				c += m;

				int range = int(h % 6.0);

				switch (range) {
				case 0: return fixed4(c, x, m, 1.0);
				case 1: return fixed4(x, c, m, 1.0);
				case 2: return fixed4(m, c, x, 1.0);
				case 3: return fixed4(m, x, c, 1.0);
				case 4: return fixed4(x, m, c, 1.0);
				case 5: return fixed4(c, m, x, 1.0);
				default: return fixed4(0, 0, 0, _HSV.w);
				}
			}

			v2f vert(appdata_full vert)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(vert.vertex);
				fixed4 colX, colY;

				switch (_Mode) {
				default:
					o.col.a = lerp(_Color1.a, _Color2.a, vert.texcoord.x);
					o.col = lerp(_Color1, _Color2, vert.texcoord.x);
					break;
				case 1:
					o.col = lerp(_Color1, _Color2, vert.texcoord.y);
					break;
				case 2:
					switch (_DoubleMode) {
					default:
						_HSV.x = lerp(_HSV_MIN.x, _HSV_MAX.x, vert.texcoord.x);
						_HSV.y = lerp(_HSV_MIN.y, _HSV_MAX.y, vert.texcoord.y);
						break;
					case 1:
						_HSV.x = lerp(_HSV_MIN.x, _HSV_MAX.x, vert.texcoord.x);
						_HSV.z = lerp(_HSV_MIN.z, _HSV_MAX.z, vert.texcoord.y);
						break;
					case 2:
						_HSV.x = lerp(_HSV_MIN.x, _HSV_MAX.x, vert.texcoord.y);
						_HSV.y = lerp(_HSV_MIN.y, _HSV_MAX.y, vert.texcoord.x);
						break;
					case 3:
						_HSV.y = lerp(_HSV_MIN.y, _HSV_MAX.y, vert.texcoord.x);
						_HSV.z = lerp(_HSV_MIN.z, _HSV_MAX.z, vert.texcoord.y);
						break;
					case 4:
						_HSV.x = lerp(_HSV_MIN.x, _HSV_MAX.x, vert.texcoord.y);
						_HSV.z = lerp(_HSV_MIN.z, _HSV_MAX.z, vert.texcoord.x);
						break;
					case 5:
						_HSV.y = lerp(_HSV_MIN.y, _HSV_MAX.y, vert.texcoord.y);
						_HSV.z = lerp(_HSV_MIN.z, _HSV_MAX.z, vert.texcoord.x);
						break;
					}

					o.col = HSV();

					break;
				case 3:
					_HSV.x = lerp(_HSV_MIN.x, _HSV_MAX.x, vert.texcoord.x);
					o.col = HSV();
					break;
				case 4:
					_HSV.x = lerp(_HSV_MIN.x, _HSV_MAX.x, vert.texcoord.y);
					o.col = HSV();
					break;
				}
				return o;
			}

			
			float4 frag(v2f i) : COLOR {
				return i.col;
			}
			
			ENDCG
		}


	}
}