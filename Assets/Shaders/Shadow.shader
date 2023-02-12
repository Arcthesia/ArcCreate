Shader "Gameplay/Shadow"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ShadowColor ("ShadowColor", Color) = (1,1,1,1)
		_From ("From", Float) = 0
		_ColorTG ("ColorTG", Color) = (1,1,1,1)
		_Shear("Shear", Vector) = (0,0,1,0)
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "RenderType" = "Transparent" "CanUseSpriteAtlas"="true" "IgnoreProjector"="true"}

		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask Off
        Lighting Off
		ZWrite On
		ZTest Always
		Cull Off
		
		Pass
		{
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
			 
			float _From;
			float4 _ShadowColor, _ColorTG;
			float4 _Shear;
			sampler2D _MainTex;
			float4 _MainTex_ST;

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
				o.vertex = UnityObjectToClipPos(vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
			    if(i.uv.y < _From || i.worldpos.z > 100 || i.worldpos.z < -100) return 0;
				float4 c = _ShadowColor; 
				c *= _ColorTG;  
				c.a *= 0.7;
				return c;
			}
			ENDCG
		}
	}
}
