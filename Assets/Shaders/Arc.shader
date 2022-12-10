Shader "Gameplay/Arc"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_ShadowColor ("ShadowColor", Color) = (1,1,1,1)
		_LowColor ("LowColor", Color) = (1,1,1,1)
		_RedColor ("RedColor", Color) = (1,1,1,1)
		_From ("From", Float) = 0
		_Selected ("Selected", Int) = 0
		_ColorTG ("ColorTG", Color) = (1,1,1,1)
		_RedValue ("RedValue", Float) = 0
		_Shear("Shear", Vector) = (0,0,1,0)
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "RenderType" = "Transparent" "CanUseSpriteAtlas"="true" "IgnoreProjector"="true"}

		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask Off
        Cull Off
        Lighting Off
		ZWrite Off
  
		// Shadow pass
		Pass
		{
			Stencil
			{
				Ref 1
				Comp notEqual
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "ColorSpace.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color  : COLOR;
				float2 uv     : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION; 
				float3 worldpos : TEXCOORD1;
				float2 uv     : TEXCOORD0;
			};
			 
			float _From;
			float4 _ShadowColor, _ColorTG;
			float4 _Shear;

			v2f vert (appdata v)
			{
				v2f o;
				float x = _Shear.x;
				float y = _Shear.y;
				float z = _Shear.z;
				float4x4 transformMatrix = float4x4(
                    1,0,x,0,
                    0,1,y,0,
                    0,0,z,0,
                    0,0,0,1);
				float4 vertex = mul(transformMatrix, v.vertex);

				o.worldpos = mul(unity_ObjectToWorld, vertex);
				vertex.y -= o.worldpos.y;
				o.worldpos.y = 0;
				o.uv = v.uv;
				o.vertex = UnityObjectToClipPos(vertex);
				return o;
			}

			half4 frag (v2f i) : SV_Target
			{
			    if(i.uv.y < _From || i.worldpos.z > 100 || i.worldpos.z < -100) return 0;
				float4 c = _ShadowColor * _ColorTG; 
				return c;
			}
			ENDCG
		}

		// Body pass
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha, One One
			Stencil
			{
				Ref 1
				Comp equal
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "ColorSpace.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color    : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION; 
				float2 uv : TEXCOORD0;
				float3 worldpos : TEXCOORD1;
			};
			 
			int _Selected;
			float _From;
			float _RedValue;
			float4 _RedColor;
			float4 _Color, _LowColor, _ColorTG;
			float4 _Shear;
            float4 _MainTex_ST;
			sampler2D _MainTex;

			v2f vert (appdata v)
			{
				v2f o;
				float x = _Shear.x;
				float y = _Shear.y;
				float z = _Shear.z;
				float4x4 transformMatrix = float4x4(
                    1,0,x,0,
                    0,1,y,0,
                    0,0,z,0,
                    0,0,0,1);
				float4 vertex = mul(transformMatrix, v.vertex);

				o.worldpos = mul(unity_ObjectToWorld, vertex);
				o.vertex = UnityObjectToClipPos(vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			half4 Highlight(half4 c)
			{
				fixed3 hsv = rgb2hsv(c.rgb);
				if(c.r<0.5) hsv.r += 0.1f;
				else hsv.g += 1.2f;
				return half4(hsv2rgb(hsv),c.a);
			}

			half4 frag (v2f i) : SV_Target
			{
			    if(i.uv.y < _From || i.worldpos.z > 100 || i.worldpos.z < -100) return 0;
				float4 c = tex2D(_MainTex,i.uv); 
				
				float4 inColor = lerp(_LowColor, _Color, clamp((i.worldpos.y - 1) / 4.5f, 0, 1));
				float4 color = lerp(inColor, _RedColor, _RedValue);

				if(_Selected == 1) 
				{
					color = Highlight(color);
				}

				c *= inColor * _ColorTG;  
				c.a *= 0.5;
				return c;
			}
			ENDCG
		}

		Pass
		{
			Stencil
			{
				Ref 1
				Comp notEqual
				Pass replace
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "ColorSpace.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color    : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION; 
				float2 uv : TEXCOORD0;
				float3 worldpos : TEXCOORD1;
			};
			 
			int _Selected;
			float _From;
			float _RedValue;
			float4 _RedColor;
			float4 _Color, _LowColor, _ColorTG;
			float4 _Shear;
            float4 _MainTex_ST;
			sampler2D _MainTex;

			v2f vert (appdata v)
			{
				v2f o;
				float x = _Shear.x;
				float y = _Shear.y;
				float z = _Shear.z;
				float4x4 transformMatrix = float4x4(
                    1,0,x,0,
                    0,1,y,0,
                    0,0,z,0,
                    0,0,0,1);
				float4 vertex = mul(transformMatrix, v.vertex);

				o.worldpos = mul(unity_ObjectToWorld, vertex);
				o.vertex = UnityObjectToClipPos(vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			half4 Highlight(half4 c)
			{
				fixed3 hsv = rgb2hsv(c.rgb);
				if(c.r<0.5) hsv.r += 0.1f;
				else hsv.g += 1.2f;
				return half4(hsv2rgb(hsv),c.a);
			}

			half4 frag (v2f i) : SV_Target
			{
			    if(i.uv.y < _From || i.worldpos.z > 100 || i.worldpos.z < -100) return 0;
				float4 c = tex2D(_MainTex,i.uv); 
				
				float4 inColor = lerp(_LowColor, _Color, clamp((i.worldpos.y - 1) / 4.5f, 0, 1));
				float4 color = lerp(inColor, _RedColor, _RedValue);

				if(_Selected == 1) 
				{
					color = Highlight(color);
				}

				c *= color * _ColorTG;  
				return c;
			}
			ENDCG
		}
	}
}
